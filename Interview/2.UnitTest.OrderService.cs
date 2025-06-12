using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Moq;

namespace Orders
{
    /*
        Parasyti unit testus order servisui
     */

    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _mockRepository;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _mockRepository = new Mock<IOrderRepository>();
            _orderService = new OrderService(_mockRepository.Object);
        }

        [Fact]
        public async Task GetAvailableIdAsync_ShouldReturnAvailable_IfOrderDoesNotExist()
        {
            // Arrange
            string orderId = "ORD-123";
            _mockRepository.Setup(r => r.GetOrderAsync(orderId)).ReturnsAsync((Order)null);

            // Act
            var result = await _orderService.GetAvailableIdAsync(orderId);

            // Assert
            Assert.True(result.IsAvailable);
            Assert.Null(result.Suggest);
        }

        [Fact]
        public async Task GetAvailableIdAsync_ShouldReturnSuggestedId_IfOrderExists()
        {
            // Arrange
            string orderId = "ORD-123";
            var existingOrder = new Order { OrderId = "ORD-123" };
            var similarOrders = new List<Order>
            {
                new Order { OrderId = "ORD-1" },
                new Order { OrderId = "ORD-100" },
                new Order { OrderId = "ORD-123" }
            };

            _mockRepository.Setup(r => r.GetOrderAsync(orderId)).ReturnsAsync(existingOrder);
            _mockRepository.Setup(r => r.FindOrdersWithSimilarIdAsync(It.IsAny<string>())).ReturnsAsync(similarOrders);

            // Act
            var result = await _orderService.GetAvailableIdAsync(orderId);

            // Assert
            Assert.False(result.IsAvailable);
            Assert.Equal("ORD-124", result.Suggest); // Tikimės naujo numerio po didžiausio 123
        }

        [Fact]
        public async Task GetAvailableIdAsync_ShouldReturnDefaultNumber_WhenNoNumberInId()
        {
            // Arrange
            string orderId = "ORD-XYZ";
            var existingOrder = new Order { OrderId = "ORD-XYZ" };
            var similarOrders = new List<Order>
            {
                new Order { OrderId = "ORD-XYZ" },
                new Order { OrderId = "ORD-5" }
            };

            _mockRepository.Setup(r => r.GetOrderAsync(orderId)).ReturnsAsync(existingOrder);
            _mockRepository.Setup(r => r.FindOrdersWithSimilarIdAsync("ORD-")).ReturnsAsync(similarOrders);

            // Act
            var result = await _orderService.GetAvailableIdAsync(orderId);

            // Assert
            Assert.False(result.IsAvailable);
            Assert.Equal("ORD-1", result.Suggest); // Kadangi nėra tinkamo numerio, siūlomas pradinis
        }

        [Fact]
        public async Task GetAvailableIdAsync_ShouldReturnNextNumber_ForCustomPatterns()
        {
            // Arrange
            string orderId = "TASK#50";
            var existingOrder = new Order { OrderId = "TASK#50" };
            var similarOrders = new List<Order>
            {
                new Order { OrderId = "TASK#1" },
                new Order { OrderId = "TASK#50" }
            };

            _mockRepository.Setup(r => r.GetOrderAsync(orderId)).ReturnsAsync(existingOrder);
            _mockRepository.Setup(r => r.FindOrdersWithSimilarIdAsync("TASK#")).ReturnsAsync(similarOrders);

            // Act
            var result = await _orderService.GetAvailableIdAsync(orderId);

            // Assert
            Assert.False(result.IsAvailable);
            Assert.Equal("TASK#51", result.Suggest);
        }
    }
     
    public interface IOrderService
    {
        Task<Order> GetOrderAsync(string orderId);
        Task<AvailableId> GetAvailableIdAsync(string orderId);
    }

    public interface IOrderRepository
    {
        Task<Order> GetOrderAsync(string orderId);
        Task<IList<Order>> FindOrdersWithSimilarIdAsync(string orderIdStartsWith);
    }
    
    public class Order
    {
        public string OrderId { get; set; } //Is privatau pakeiciau i public "private set;"
    }

    public class AvailableId
    {
        public bool IsAvailable { get; set; }
        public string Suggest { get; set; }
    }

    public class OrderRepository : IOrderRepository
    {
        public async Task<Order> GetOrderAsync(string orderId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IList<Order>> FindOrdersWithSimilarIdAsync(string orderIdStartsWith)
        {
            throw new System.NotImplementedException();
        }
    }

    public class OrderService: IOrderService
    {
        //const string Pattern = @"([a-zA-Z])+([\s\-:#=]*)(\d*)"; // "str, simb, num", minimaiai pakeiciau regex israiska
        const string Pattern = @"([a-zA-Z]+)([\s\-:#=]*)(\d*)";


        private readonly IOrderRepository _repository;

        public OrderService(IOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<AvailableId> GetAvailableIdAsync(string orderId)
        {
            var order = await _repository.GetOrderAsync(orderId).ConfigureAwait(false);

            if (order == null)
                return new AvailableId { IsAvailable = true};

            var match = Regex.Match(orderId, Pattern);
            
            var orderIdStartsWith = match.Groups[1].Captures[0].Value + match.Groups[2].Captures.FirstOrDefault()?.Value;
            var list = await _repository.FindOrdersWithSimilarIdAsync(orderIdStartsWith).ConfigureAwait(false);

            var lastOrder = list.OrderBy(l => l.OrderId).Last();
            
            var match2 = Regex.Match(lastOrder.OrderId, Pattern);
            var numberString = match2.Groups[3].Captures.FirstOrDefault()?.Value;
            if (!int.TryParse(numberString, out var number))
            {
                number = 0;
            }

            var format = "";
            if (!string.IsNullOrEmpty(numberString))
            {
                format = $"D{numberString.Length}";
            }

            number++;
            return new AvailableId
            {
                IsAvailable = false,
                Suggest = orderIdStartsWith + number.ToString(format)
            };
        }

        public Task<Order> GetOrderAsync(string orderId)
        {
            return _repository.GetOrderAsync(orderId);
        }
    }
}
