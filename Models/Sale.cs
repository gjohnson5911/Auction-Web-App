using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Auction.Models
{
    public class Sale 
    {
        [Key]
        public int Id {get; set;}

        [Required]
        [MinLength(2)]
        public string ProductName {get; set;}

        [Required]
        public string Description {get; set;}

        [Required]
        [DataType(DataType.Currency)]
        public double StartingBid {get; set;}

        [Required]
        public DateTime EndDate {get; set;}

        [DataType(DataType.Currency)]
        public double TopBid {get; set;}

        public int OwnerId {get; set;}

        public User Owner {get; set;}

        public int TopBidUserId {get; set;}

        public User TopBidUser {get; set;}

        public DateTime Created_At {get; set;}

        public DateTime Updated_At {get; set;}

        public int Remaining {get; set;}

        public int DaysRemaining()
        {
            TimeSpan compareDate = this.EndDate - DateTime.Now;
            return compareDate.Days;
        }
    }
    
}