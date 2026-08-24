using Gymble.Models;
using Gymble.Repositories;
using Gymble.Services;
using Gymble.ViewModels;
using System.Data.SQLite;

var tests = new (string Name, Func<Task> Run)[]
{
    ("name or code search", TestNameOrCodeSearch),
    ("status search", TestStatusSearch),
    ("category search", TestCategorySearch),
    ("period and count search", TestPeriodAndCountSearch),
    ("usage value range search", TestUsageValueRangeSearch),
    ("price range search", TestPriceRangeSearch),
    ("start type search", TestStartTypeSearch),
    ("combined component search", TestCombinedComponentSearch),
    ("package same component policy", TestPackageSameComponentPolicy),
    ("duplicate product prevention", TestDuplicateProductPrevention),
    ("filter reset", TestFilterReset),
    ("min greater than max validation", TestMinGreaterThanMaxValidation),
    ("latest rapid search wins", TestLatestRapidSearchWins)
};

var failures = 0;

foreach (var test in tests)
{
    try
    {
        await test.Run();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception ex)
    {
        failures++;
        Console.WriteLine($"FAIL {test.Name}: {ex.Message}");
    }
}

if (failures > 0)
    Environment.Exit(1);

static async Task TestNameOrCodeSearch()
{
    await WithSeededRepository(async (repo, seed) =>
    {
        var byCode = await repo.SearchAsync(new ProductSearch { NameOrCode = "PT-10" });
        AssertContainsOnly(byCode, seed.PtCount10);

        var byName = await repo.SearchAsync(new ProductSearch { NameOrCode = "Gym 30" });
        AssertContainsOnly(byName, seed.GymPeriod);
    });
}

static async Task TestStatusSearch()
{
    await WithSeededRepository(async (repo, seed) =>
    {
        var result = await repo.SearchAsync(new ProductSearch
        {
            Statuses = [ProductStatus.Stopped]
        });

        AssertContainsOnly(result, seed.LockerPeriod);
    });
}

static async Task TestCategorySearch()
{
    await WithSeededRepository(async (repo, seed) =>
    {
        var result = await repo.SearchAsync(new ProductSearch
        {
            SelectedCategory = ProductCategory.Wear
        });

        AssertContainsOnly(result, seed.WearPeriod);
    });
}

static async Task TestPeriodAndCountSearch()
{
    await WithSeededRepository(async (repo, seed) =>
    {
        var period = await repo.SearchAsync(new ProductSearch
        {
            UsageType = ProductUsageType.Period
        });

        AssertContains(period, seed.GymPeriod, seed.LockerPeriod, seed.PackagePtGym, seed.SplitPackage, seed.WearPeriod);

        var count = await repo.SearchAsync(new ProductSearch
        {
            UsageType = ProductUsageType.Count
        });

        AssertContains(count, seed.PtCount10, seed.PackagePtGym, seed.SplitPackage, seed.DoublePtPackage);
    });
}

static async Task TestUsageValueRangeSearch()
{
    await WithSeededRepository(async (repo, seed) =>
    {
        var result = await repo.SearchAsync(new ProductSearch
        {
            UsageType = ProductUsageType.Period,
            MinUsageValue = 30,
            MaxUsageValue = 30
        });

        AssertContains(result, seed.GymPeriod, seed.LockerPeriod, seed.PackagePtGym, seed.SplitPackage);
        AssertDoesNotContain(result, seed.WearPeriod);
    });
}

static async Task TestPriceRangeSearch()
{
    await WithSeededRepository(async (repo, seed) =>
    {
        var result = await repo.SearchAsync(new ProductSearch
        {
            MinPrice = 450_000,
            MaxPrice = 560_000
        });

        AssertContainsOnly(result, seed.PtCount10, seed.PackagePtGym);
    });
}

static async Task TestStartTypeSearch()
{
    await WithSeededRepository(async (repo, seed) =>
    {
        var result = await repo.SearchAsync(new ProductSearch
        {
            StartType = ProductStartType.SelectDate
        });

        AssertContainsOnly(result, seed.PtCount10);
    });
}

static async Task TestCombinedComponentSearch()
{
    await WithSeededRepository(async (repo, seed) =>
    {
        var result = await repo.SearchAsync(new ProductSearch
        {
            SelectedCategory = ProductCategory.PT,
            UsageType = ProductUsageType.Count,
            MinUsageValue = 10,
            MaxUsageValue = 10,
            StartType = ProductStartType.SelectDate
        });

        AssertContainsOnly(result, seed.PtCount10);
    });
}

static async Task TestPackageSameComponentPolicy()
{
    await WithSeededRepository(async (repo, seed) =>
    {
        var result = await repo.SearchAsync(new ProductSearch
        {
            SelectedCategory = ProductCategory.PT,
            UsageType = ProductUsageType.Count,
            MinUsageValue = 10
        });

        AssertContains(result, seed.PtCount10, seed.PackagePtGym, seed.DoublePtPackage);
        AssertDoesNotContain(result, seed.SplitPackage);
    });
}

static async Task TestDuplicateProductPrevention()
{
    await WithSeededRepository(async (repo, seed) =>
    {
        var result = await repo.SearchAsync(new ProductSearch
        {
            SelectedCategory = ProductCategory.PT,
            UsageType = ProductUsageType.Count
        });

        var ids = result.Select(x => x.Id).ToList();
        AssertEqual(ids.Count, ids.Distinct().Count(), "Search returned duplicate product rows.");
        AssertEqual(1, ids.Count(x => x == seed.DoublePtPackage), "Double PT package should appear once.");
    });
}

static async Task TestFilterReset()
{
    var service = new RecordingProductService();
    var vm = new ProductViewModel(service);

    await WaitUntil(() => service.SearchCallCount > 0);

    vm.SelectedCategory = ProductCategory.PT;
    vm.SelectedUsageType = ProductUsageType.Count;
    vm.SelectedStartType = ProductStartType.FixedDate;
    vm.MinUsageValue = "20";
    vm.MaxUsageValue = "10";
    vm.MinPrice = "500";
    vm.MaxPrice = "100";

    vm.ResetFilters();
    await WaitUntil(() => service.SearchCallCount >= 2);

    AssertEqual(null, vm.SelectedCategory, "Category should reset to all.");
    AssertEqual(ProductUsageType.All, vm.SelectedUsageType, "Usage type should reset to all.");
    AssertEqual(null, vm.SelectedStartType, "Start type should reset to all.");
    AssertEqual("", vm.MinUsageValue, "Min usage should reset.");
    AssertEqual("", vm.MaxUsageValue, "Max usage should reset.");
    AssertEqual("", vm.MinPrice, "Min price should reset.");
    AssertEqual("", vm.MaxPrice, "Max price should reset.");
    AssertEqual("", vm.UsageValueError, "Usage validation error should reset.");
    AssertEqual("", vm.PriceError, "Price validation error should reset.");
    AssertTrue(vm.StatusFilters[0].IsChecked, "First status should be selected after reset.");
    AssertTrue(vm.StatusFilters.Skip(1).All(x => !x.IsChecked), "Only first status should be selected after reset.");
}

static async Task TestMinGreaterThanMaxValidation()
{
    var service = new RecordingProductService();
    var vm = new ProductViewModel(service);

    await WaitUntil(() => service.SearchCallCount > 0);

    vm.MinUsageValue = "20";
    await WaitUntil(() => service.LastSearch?.MinUsageValue == 20);

    var callsBeforeInvalid = service.SearchCallCount;
    vm.MaxUsageValue = "10";
    await Task.Delay(80);

    AssertTrue(!string.IsNullOrWhiteSpace(vm.UsageValueError), "Usage validation error should be shown.");
    AssertEqual(callsBeforeInvalid, service.SearchCallCount, "Invalid usage range should not execute search.");

    vm.MinPrice = "500";
    vm.MaxPrice = "100";
    await Task.Delay(80);

    AssertTrue(!string.IsNullOrWhiteSpace(vm.PriceError), "Price validation error should be shown.");
}

static async Task TestLatestRapidSearchWins()
{
    var service = new RaceProductService();
    var vm = new ProductViewModel(service);

    await WaitUntil(() => service.SearchCallCount > 0);

    vm.SearchInput = "slow";
    var slowSearch = vm.SearchProduct();

    await Task.Delay(20);

    vm.SearchInput = "fast";
    var fastSearch = vm.SearchProduct();

    await Task.WhenAll(slowSearch, fastSearch);

    AssertEqual(1, vm.Items.Count, "Latest search should leave one result.");
    AssertEqual("FAST", vm.Items[0].Code, "Older slow result should not overwrite latest search.");
}

static async Task WithSeededRepository(Func<ProductRepository, SeedData, Task> test)
{
    using var db = CreateDatabase();
    var seed = await SeedProducts(db.Repository);
    await test(db.Repository, seed);
}

static TestDatabase CreateDatabase()
{
    var path = Path.Combine(Path.GetTempPath(), $"gymble-product-search-{Guid.NewGuid():N}.db");
    SQLiteConnection.CreateFile(path);

    SQLiteConnection OpenConnection()
    {
        var conn = new SQLiteConnection($"Data Source={path};Version=3;");
        conn.Open();
        return conn;
    }

    using (var conn = OpenConnection())
    {
        ExecuteNonQuery(conn, SqlProductQuery.CREATE_PRODUCT_TABLE);
        ExecuteNonQuery(conn, SqlProductComponentQuery.CREATE_PRODUCT_COMPONENT_TABLE);
    }

    return new TestDatabase(path, new ProductRepository(OpenConnection));
}

static async Task<SeedData> SeedProducts(ProductRepository repo)
{
    var gymPeriod = await AddProduct(repo, "GYM-30", "Gym 30 Days", ProductSaleType.Single, 100_000, ProductStatus.OnSale,
        Component("Gym", ProductCategory.Gym, ProductUsageType.Period, 30, ProductStartType.Immediate));

    var ptCount10 = await AddProduct(repo, "PT-10", "PT 10 Pack", ProductSaleType.Single, 500_000, ProductStatus.OnSale,
        Component("PT", ProductCategory.PT, ProductUsageType.Count, 10, ProductStartType.SelectDate));

    var lockerPeriod = await AddProduct(repo, "LOCKER-30", "Locker 30 Days", ProductSaleType.Single, 30_000, ProductStatus.Stopped,
        Component("Locker", ProductCategory.Locker, ProductUsageType.Period, 30, ProductStartType.FirstCheckIn));

    var packagePtGym = await AddProduct(repo, "PKG-PT-GYM", "Package PT Gym", ProductSaleType.Package, 550_000, ProductStatus.OnSale,
        Component("PT", ProductCategory.PT, ProductUsageType.Count, 10, ProductStartType.FixedDate),
        Component("Gym", ProductCategory.Gym, ProductUsageType.Period, 30, ProductStartType.Immediate));

    var splitPackage = await AddProduct(repo, "PKG-SPLIT", "Split Package", ProductSaleType.Package, 400_000, ProductStatus.OnSale,
        Component("PT", ProductCategory.PT, ProductUsageType.Period, 30, ProductStartType.Immediate),
        Component("Gym", ProductCategory.Gym, ProductUsageType.Count, 10, ProductStartType.Immediate));

    var doublePtPackage = await AddProduct(repo, "PKG-DOUBLE-PT", "Double PT Package", ProductSaleType.Package, 600_000, ProductStatus.OnSale,
        Component("PT A", ProductCategory.PT, ProductUsageType.Count, 10, ProductStartType.Immediate),
        Component("PT B", ProductCategory.PT, ProductUsageType.Count, 20, ProductStartType.FixedDate));

    var wearPeriod = await AddProduct(repo, "WEAR-7", "Wear 7 Days", ProductSaleType.Single, 20_000, ProductStatus.OnSale,
        Component("Wear", ProductCategory.Wear, ProductUsageType.Period, 7, ProductStartType.Immediate));

    return new SeedData(gymPeriod, ptCount10, lockerPeriod, packagePtGym, splitPackage, doublePtPackage, wearPeriod);
}

static async Task<int> AddProduct(
    ProductRepository repo,
    string code,
    string name,
    ProductSaleType saleType,
    int price,
    ProductStatus status,
    params ProductComponent[] components)
{
    var now = DateTime.Now;
    var product = new Product
    {
        Code = code,
        Name = name,
        SaleType = saleType,
        Price = price,
        Status = status,
        CreatedAt = now,
        UpdatedAt = now
    };

    return (int)await repo.InsertProductWithComponentsAsync(product, components);
}

static ProductComponent Component(
    string name,
    ProductCategory category,
    ProductUsageType usageType,
    int usageValue,
    ProductStartType startType)
{
    return new ProductComponent
    {
        Name = name,
        Category = category,
        UsageType = usageType,
        UsageValue = usageValue,
        StartType = startType,
        FixedStartDate = startType == ProductStartType.FixedDate ? DateTime.Today : null
    };
}

static void ExecuteNonQuery(SQLiteConnection conn, string sql)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = sql;
    cmd.ExecuteNonQuery();
}

static void AssertContainsOnly(IReadOnlyList<Product> products, params int[] expectedIds)
{
    var actual = products.Select(x => x.Id).OrderBy(x => x).ToArray();
    var expected = expectedIds.OrderBy(x => x).ToArray();

    if (!actual.SequenceEqual(expected))
        throw new InvalidOperationException($"Expected ids [{string.Join(", ", expected)}], got [{string.Join(", ", actual)}].");
}

static void AssertContains(IReadOnlyList<Product> products, params int[] expectedIds)
{
    var actual = products.Select(x => x.Id).ToHashSet();

    foreach (var expectedId in expectedIds)
    {
        if (!actual.Contains(expectedId))
            throw new InvalidOperationException($"Expected id {expectedId} to be present.");
    }
}

static void AssertDoesNotContain(IReadOnlyList<Product> products, params int[] unexpectedIds)
{
    var actual = products.Select(x => x.Id).ToHashSet();

    foreach (var unexpectedId in unexpectedIds)
    {
        if (actual.Contains(unexpectedId))
            throw new InvalidOperationException($"Expected id {unexpectedId} to be absent.");
    }
}

static void AssertEqual<T>(T expected, T actual, string message)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"{message} Expected {expected}, got {actual}.");
}

static void AssertTrue(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

static async Task WaitUntil(Func<bool> condition)
{
    var deadline = DateTime.UtcNow.AddSeconds(3);

    while (DateTime.UtcNow < deadline)
    {
        if (condition())
            return;

        await Task.Delay(20);
    }

    throw new TimeoutException("Timed out waiting for test condition.");
}

static class TestHelpers
{
    public static ProductSearch CloneSearch(ProductSearch source)
    {
        return new ProductSearch
        {
            NameOrCode = source.NameOrCode,
            SaleType = source.SaleType,
            SelectedCategory = source.SelectedCategory,
            Statuses = source.Statuses?.ToList(),
            UsageType = source.UsageType,
            MinUsageValue = source.MinUsageValue,
            MaxUsageValue = source.MaxUsageValue,
            MinPrice = source.MinPrice,
            MaxPrice = source.MaxPrice,
            IsFavorite = source.IsFavorite,
            StartType = source.StartType,
            SortBy = source.SortBy,
            Desc = source.Desc,
            Take = source.Take,
            Skip = source.Skip
        };
    }
}

sealed record SeedData(
    int GymPeriod,
    int PtCount10,
    int LockerPeriod,
    int PackagePtGym,
    int SplitPackage,
    int DoublePtPackage,
    int WearPeriod);

sealed class TestDatabase : IDisposable
{
    public TestDatabase(string path, ProductRepository repository)
    {
        Path = path;
        Repository = repository;
    }

    public string Path { get; }
    public ProductRepository Repository { get; }

    public void Dispose()
    {
        try
        {
            File.Delete(Path);
        }
        catch
        {
        }
    }
}

sealed class RecordingProductService : IProductService
{
    public int SearchCallCount { get; private set; }
    public ProductSearch? LastSearch { get; private set; }

    public Task<IReadOnlyList<Product>> SearchAsync(ProductSearch q, CancellationToken ct = default)
    {
        SearchCallCount++;
        LastSearch = TestHelpers.CloneSearch(q);
        return Task.FromResult<IReadOnlyList<Product>>([]);
    }

    public Task<Product?> GetByIdAsync(long productId, CancellationToken ct = default) => Task.FromResult<Product?>(null);
    public Task<IReadOnlyList<ProductComponent>> GetComponentsAsync(long productId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<ProductComponent>>([]);
    public Task<long> AddAsync(ProductUpsertRequest request, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UpdateAsync(ProductUpsertRequest request, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(long productId, CancellationToken ct = default) => throw new NotSupportedException();
}

sealed class RaceProductService : IProductService
{
    public int SearchCallCount { get; private set; }

    public async Task<IReadOnlyList<Product>> SearchAsync(ProductSearch q, CancellationToken ct = default)
    {
        SearchCallCount++;

        var key = q.NameOrCode ?? "init";
        var delay = key switch
        {
            "slow" => 250,
            "fast" => 30,
            _ => 5
        };

        await Task.Delay(delay, ct);

        return
        [
            new Product
            {
                Id = key == "fast" ? 2 : 1,
                Code = key.ToUpperInvariant(),
                Name = key,
                Status = ProductStatus.OnSale
            }
        ];
    }

    public Task<Product?> GetByIdAsync(long productId, CancellationToken ct = default) => Task.FromResult<Product?>(null);
    public Task<IReadOnlyList<ProductComponent>> GetComponentsAsync(long productId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<ProductComponent>>([]);
    public Task<long> AddAsync(ProductUpsertRequest request, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UpdateAsync(ProductUpsertRequest request, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(long productId, CancellationToken ct = default) => throw new NotSupportedException();
}
