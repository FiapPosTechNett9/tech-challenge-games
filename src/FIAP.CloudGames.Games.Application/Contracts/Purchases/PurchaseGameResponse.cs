using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.CloudGames.Games.Application.Contracts.Purchases
{
    public class PurchaseGameResponse
    {
        public Guid OrderId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid GameId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
    }
}
