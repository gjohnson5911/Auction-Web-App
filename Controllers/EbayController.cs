using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Auction.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Auction.Controllers
{
    public class EbayController:Controller 
    {
        public AuctionContext context;

        public EbayController(AuctionContext cont)
        {
            context = cont;
        }

        //render login and registration page
        [HttpGet]
        [Route("")]
        public IActionResult LogReg()
        {
            HttpContext.Session.Clear();
            return View();
        }

        //attempt to log returning user in
        //log in and send to home page if success
        //send to logreg if failed w/ error message
        [HttpPost("Login")]
        public IActionResult LogIn(string userAttempt, string passAttempt)
        {
            //verify both fields have data
            if(userAttempt != null && passAttempt != null)
            {
                //search for user by username
                User logUser = context.Users.FirstOrDefault(user => user.Username == userAttempt);
                if(logUser != null)
                {
                    //test password
                    PasswordHasher<User> hasher = new PasswordHasher<User>();
                    if(hasher.VerifyHashedPassword(logUser, logUser.Password, passAttempt) != 0)
                    {
                        //set session and redirect to home
                        HttpContext.Session.SetInt32("UserId", logUser.Id);
                        return RedirectToAction("home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Username or password does not match. Please try again.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Username or password does not match. Please try again.");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Please fill out Log In form completely.");
            }
            return View("logreg");
        }

        //attempt to register new user, log in and send to home page if success, send to logreg if failed w/ error message
        [HttpPost("Register")]
        public IActionResult Register(User newUserAttempt)
        {
            //check modelstate
            if(ModelState.IsValid)
            {
                //check if username has been used
                User testName = context.Users.FirstOrDefault(u=>u.Username == newUserAttempt.Username);
                if(testName == null)
                {
                    //Hash password
                    PasswordHasher<User> hasher = new PasswordHasher<User>();
                    newUserAttempt.Password = hasher.HashPassword(newUserAttempt, newUserAttempt.Password);
                    //set created_at and updated_at
                    newUserAttempt.Created_At = DateTime.Now;
                    newUserAttempt.Updated_At = DateTime.Now;
                    //add user to DB and savechanges
                    context.Users.Add(newUserAttempt);
                    context.SaveChanges();
                    //set session id
                    HttpContext.Session.SetInt32("UserId", newUserAttempt.Id);
                    //send to home
                    return RedirectToAction("Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Username is already taken, please try again.");
                }
            }
            return View("logreg");
        }

        //render home page with all auctions
        [HttpGet("Auctions/Current")]
        public IActionResult Home()
        {
            //check that user is logged in 
            if(HttpContext.Session.GetInt32("UserId") != null)
            {
                //get current user
                User currentUser = context.Users.FirstOrDefault(u=>u.Id == HttpContext.Session.GetInt32("UserId"));
                //get all sales
                IEnumerable<Sale> allSales = context.Sales.Include(s=>s.TopBidUser).Include(s=>s.Owner).ToList();
                //check for any auctions that have ended and update DB accordingly
                User winner;
                User owner;
                foreach(Sale act in allSales)
                {
                    if(act.EndDate <= DateTime.Now.Date)
                    {
                        //get top bidder
                        winner = act.TopBidUser;
                        //get auction owner
                        owner = act.Owner;
                        //subtract winning bid from user's biddingamount and add to owner's wallet
                        winner.BiddingAmount -= act.TopBid;
                        owner.WalletAmount += act.TopBid;
                        //delete sale
                        context.Sales.Remove(act);
                    }
                }
                //save changes and get list of all sales again
                context.SaveChanges();
                allSales = context.Sales.Include(s=>s.Owner);
                //calculate how much time remaining for each sale
                int days;
                foreach(Sale act in allSales)
                {
                    //.date?
                    days = (act.EndDate - DateTime.Now).Days;
                    act.Remaining = days+1;
                }
                //sort by time remaining.
                allSales.OrderBy(s=>s.Remaining);
                //set data in ViewBag
                ViewBag.User = currentUser;
                ViewBag.Sales = allSales;
                return View();
            }
            else
            {
                return RedirectToAction("logreg");
            }

        }

        //render new auction page
        [HttpGet("Auctions/Create")]
        public IActionResult CreateAuction()
        {
            //check that user is logged in
            if(HttpContext.Session.GetInt32("UserId") != null)
            {
                //get current user
                User currentUser = context.Users.FirstOrDefault(u=>u.Id == (int)HttpContext.Session.GetInt32("UserId"));
                //get current date for min value in endDate field
                DateTime today = DateTime.Now;
                //set data in ViewBag
                ViewBag.User = currentUser;
                ViewBag.Today = today.Date.ToString();
                return View();
            }
            else
            {
                return RedirectToAction("logreg");
            }
        }

        //render individual auction page
        [HttpGet("Auctions/{saleId}")]
        public IActionResult AuctionPage(int saleId)
        {
            //check that user is logged in
            if(HttpContext.Session.GetInt32("UserId") != null)
            {
                //get current user
                User currentUser = context.Users.FirstOrDefault(u=>u.Id == (int)HttpContext.Session.GetInt32("UserId"));
                //get current sale and set time remaining
                Sale currentSale = context.Sales.Include(s=>s.TopBidUser).Include(s=>s.Owner).FirstOrDefault(s=>s.Id == saleId);
                int days = (currentSale.EndDate - DateTime.Now).Days;
                currentSale.Remaining = days+1;
                //set data in ViewBag
                ViewBag.User = currentUser;
                ViewBag.Sale = currentSale;
                return View();
            }
            else
            {
                return RedirectToAction("logreg");
            }
        }

        //create new auction
        [HttpPost("Auctions/New")]
        public IActionResult NewAuction(Sale newSaleAttempt)
        {
            //check modelstate
            if(ModelState.IsValid)
            {
                //check that end date is after today's date
                if(newSaleAttempt.EndDate <= DateTime.Now)
                {
                    ModelState.AddModelError(string.Empty, "End date must be no earlier than tomorrow's date.");
                }
                else if(newSaleAttempt.StartingBid <=0)
                {
                    ModelState.AddModelError(string.Empty, "Starting bid must be at least $0.01");
                }
                else
                {
                newSaleAttempt.TopBid = 0.00;
                newSaleAttempt.OwnerId = (int)HttpContext.Session.GetInt32("UserId");
                newSaleAttempt.Created_At = DateTime.Now;
                newSaleAttempt.Updated_At = DateTime.Now;
                newSaleAttempt.TopBidUserId = (int)HttpContext.Session.GetInt32("UserId");
                //add to DB and save
                context.Sales.Add(newSaleAttempt);
                context.SaveChanges();
                //send to individual auction page
                return RedirectToAction("AuctionPage", new {saleId = newSaleAttempt.Id});
                }
            }
            return View("CreateAuction");
        }

        //delete auction
        [HttpGet("Auctions/{saleId}/delete")]
        public IActionResult DeleteAuction(int saleId)
        {
            //get sale being deleted
            Sale deleteSale = context.Sales.Include(s=>s.TopBidUser).FirstOrDefault(s=>s.Id == saleId);
            //return top bid amount to topbid user's wallet and subtract from biddingamount
            User topBidder = deleteSale.TopBidUser;
            topBidder.WalletAmount += deleteSale.TopBid;
            topBidder.BiddingAmount -= deleteSale.TopBid;
            //remove and save changes
            context.Sales.Remove(deleteSale);
            context.SaveChanges();
            //redirect to home
            return RedirectToAction("Home");
        }

        //bid on auction
        [HttpPost("Auctions/{saleId}/Bid")]
        public IActionResult Bid(int saleId, double newBid)
        {
            //get current user
            User currentUser = context.Users.FirstOrDefault(u=>u.Id == (int)HttpContext.Session.GetInt32("UserId"));
            //get current sale
            Sale currentSale = context.Sales.Include(s=>s.TopBidUser).FirstOrDefault(s=>s.Id == saleId);
            //check if user has enough to make the bid
            if(currentUser.WalletAmount > newBid)
            {
                //check if newbid is more than the sale's current top bid
                if(currentSale.TopBid < newBid)
                {
                    //subtract previous topbid from topbiduser's biddingamount and add to walletamount
                    currentSale.TopBidUser.BiddingAmount -= currentSale.TopBid;
                    currentSale.TopBidUser.WalletAmount += currentSale.TopBid;
                    //subtract newbid from user's walletamount and add to biddingamount
                    currentUser.WalletAmount -= newBid;
                    currentUser.BiddingAmount += newBid;
                    //set sale's top bid to newbid, and change topbidid
                    currentSale.TopBid = newBid;
                    currentSale.TopBidUserId = currentUser.Id;
                    //save changes
                    context.SaveChanges();
                    //redirect to bid's individual page
                    return RedirectToAction("AuctionPage", new {saleId = currentSale.Id});
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Your new bid must be higher than the current highest bid.");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "You do not have enough in your wallet for that bid...");
            }
            //get current user
            currentUser = context.Users.FirstOrDefault(u=>u.Id == (int)HttpContext.Session.GetInt32("UserId"));
            //get current sale and set time remaining
            currentSale = context.Sales.Include(s=>s.TopBidUser).Include(s=>s.Owner).FirstOrDefault(s=>s.Id == saleId);
            int days = (currentSale.EndDate - DateTime.Now).Days;
            currentSale.Remaining = days+1;
            //set data in ViewBag
            ViewBag.User = currentUser;
            ViewBag.Sale = currentSale;
            return View("AuctionPage", new {saleId = currentSale.Id});
        }
    }
}