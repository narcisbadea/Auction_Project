﻿using Auction_Project.Models.Bids;

namespace Auction_Project.Models.Items
{
    public class ItemResponseForAdminDTO
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
       
        public string? Desc { get; set; }

        public decimal? InitialPrice { get; set; }

        public double? EndTime { get; set; }

        public List<int>? Gallery { get; set; }

        public List<BidResponseForAdminDTO>? BidsOnItem { get; set; }

    }
}
