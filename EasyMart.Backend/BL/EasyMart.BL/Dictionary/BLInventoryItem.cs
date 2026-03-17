using BASE.Service.Core.BL;
using BASE.Service.Core.Model;
using EasyMart.BLBase;
using EasyMart.DL.Dictionary;
using EasyMart.Model.Dictionary;
using System.Diagnostics;

namespace EasyMart.BL.Dictionary
{
    public class BLInventoryItem : BLBaseDictionary<DLInventoryItem>
    {
        public BLInventoryItem(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override DLInventoryItem CreateDL()
        {
            return new DLInventoryItem(_mySQLService);
        }

        public async Task<ServiceResponse> InsertBatch()
        {
            var listInsert = new List<InventoryItem>();
            var random = new Random();

            for (int i = 0; i < 3; i++)
            {
                var item = new InventoryItem
                {
                    InventoryItemID = Guid.NewGuid(),
                    ReleaseMethod = random.Next(1, 4), // 1-3
                    UnitID = Guid.NewGuid(),
                    UnitName = GetRandomUnitName(random),
                    InventoryItemCode = $"VT{(i + 1):D4}", // VT0001, VT0002...
                    InventoryItemName = $"Kickoff 2026",
                    InventoryItemType = random.Next(0, 3), // 0-2
                    InventoryItemCategoryIDList = $"{Guid.NewGuid()},{Guid.NewGuid()}",
                    InventoryItemCategoryCodeList = "CATEGORY1,CATEGORY2",
                    InventoryItemCategoryNameList = "Danh mục 1, Danh mục 2",
                    MaximumStock = random.Next(50, 200),
                    MinimumStock = random.Next(5, 20),
                    QuantityBalance = random.Next(10, 100),
                    WarrantyTimeUnit = random.Next(1, 4), // 1-3
                    InventoryItemSource = GetRandomSource(random),
                    Description = $"Mô tả chi tiết cho sản phẩm {i + 1}",
                    Images = "[\"https://example.com/image1.jpg\", \"https://example.com/image2.jpg\"]",
                    Inactive = random.Next(0, 2) == 1, // true/false
                    UnitList = "[{\"unitID\":\"" + Guid.NewGuid() + "\",\"unitName\":\"Cái\",\"conversionRate\":1}]",
                    BuyPrice = random.Next(100000, 5000000),
                    SellPrice = random.Next(150000, 6000000),
                    BackEndFormula = "SellPrice * 1.1",
                    FrontEndFormula = "BuyPrice + 500000",
                    DICustomField1 = $"Custom 1 - {i}",
                    DICustomField2 = $"Custom 2 - {i}",
                    CreatedDate = DateTime.Now.AddDays(-random.Next(1, 365)),
                    CreatedBy = "admin@example.com",
                    ModifiedDate = DateTime.Now,
                    ModifiedBy = "system@example.com",
                    IsFollowSerialNumber = random.Next(0, 2) == 1,
                    IsAllowDuplicateSerialNumber = random.Next(0, 2) == 1,
                    WarrantyTime = random.Next(6, 36),
                    BaseOnFormula = random.Next(1, 3),
                    QuantityAvailable = random.Next(5, 80),
                    InventoryCombo = "[{\"comboItemID\":\"" + Guid.NewGuid() + "\",\"comboItemName\":\"Phụ kiện\",\"quantity\":1}]"
                };

                listInsert.Add(item);
            }

            var st = new Stopwatch();
            st.Start();
            ServiceResponse serviceResponse = await SaveListDataAsync(listInsert);
            st.Stop();
            return new ServiceResponse()
            {
                Data = st
            };
        }

        // Helper methods
        private string GetRandomUnitName(Random random)
        {
            var units = new[] { "Cái", "Chiếc", "Bộ", "Hộp", "Thùng", "Kg", "Lít" };
            return units[random.Next(units.Length)];
        }

        private string GetRandomSource(Random random)
        {
            var sources = new[]
            {
                "Nhập khẩu từ Trung Quốc",
                "Sản xuất trong nước",
                "Nhập khẩu từ Nhật Bản",
                "Nhập khẩu từ Hàn Quốc",
                "Sản xuất tại Việt Nam"
            };
            return sources[random.Next(sources.Length)];
        }
    }
}
