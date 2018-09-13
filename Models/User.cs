using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Auction.Models
{
    public class User 
    {
        [Key]
        public int Id {get; set;}

        [Required]
        [MinLength(4)]
        [MaxLength(20)]
        public string Username {get; set;}

        [Required]
        public string FirstName {get; set;}

        [Required]
        public string LastName {get; set;}

        [Required]
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string Password {get; set;}

        [Required]
        [NotMapped]
        [DataType(DataType.Password)]
        [Compare("Password")]    
        public string Confirm {get; set;}

        [DataType(DataType.Currency)]
        public double WalletAmount {get; set;}

        [DataType(DataType.Currency)]
        public double BiddingAmount {get; set;}

        public DateTime Created_At {get; set;}

        public DateTime Updated_At {get; set;}

        public User(){
            WalletAmount = 1000.00;
        }
        

    }
}