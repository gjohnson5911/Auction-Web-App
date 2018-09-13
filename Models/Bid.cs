using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Auction.Models
{
    public class Bid {
        [Key]
        public int Id {get; set;}

        public double Amount {get; set;}

        public int BidUserId {get; set;}

        public User BidUser {get; set;}

        public DateTime Created_At {get; set;}

        public DateTime Updated_At {get; set;}
    }
}