//SHOPPING CART - STEP 05
//Create a new empty Controller named ShoppingCartController
//Add the using statements below:

using StoreFront.DATA.EF.Models;//Added for access to the DB context
using Microsoft.AspNetCore.Identity;//Added for access to the UserManager object
using StoreFront.UI.MVC.Models;//Added for the CartItemViewModel
using Newtonsoft.Json;//Added for easier management of the shopping cart

using Microsoft.AspNetCore.Mvc;
using System.Xml;
using NuGet.Protocol;

namespace StoreFront.UI.MVC.Controllers
{
    public class ShoppingCartController : Controller
    {
        //SHOPPING CART - STEP 06
        //Add the fields and the constructor below:

        //FIELDS
        private readonly KonohaExpressContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        //CONSTRUCTOR
        public ShoppingCartController(KonohaExpressContext context, UserManager<IdentityUser> userManager)
        {
            //Assignment
            _context = context;
            _userManager = userManager;
        }

        //SHOPPING CART - STEP 08
        //Add logic to the Index action to prepare Shopping Cart items to display to the user
        public IActionResult Index()
        {
            //Retrieve the contents from the Session shopping cart (JSON string "cart") and convert
            //them to C# using Newtonsoft.Json. After converting to C#, we can pass the collection
            //of cart contents back to the strongly-typed view to display.

            //retrieve cart contents
            var sessionCart = HttpContext.Session.GetString("cart");

            //create the shell for local (C# version) cart:
            Dictionary<int, CartItemViewModel> shoppingCart = null;

            //check to see if the session cart was null or empty
            if (sessionCart == null || sessionCart.Count() == 0)
            {
                //User either hasn't put anything in their cart, or they have removed all items
                //set shopping cart to an empty Dictionary
                shoppingCart = new Dictionary<int, CartItemViewModel>();

                ViewBag.Message = "There are no items in your cart.";
            }
            else
            {
                ViewBag.Message = null;
                //deserialize the cart contents from JSON to C#
                shoppingCart = JsonConvert.DeserializeObject<Dictionary<int, CartItemViewModel>>(sessionCart);
            }

            return View(shoppingCart);
        }

        //SHOPPING CART - STEP 07
        //Code the AddToCart Action and test in debug mode
        public IActionResult AddToCart(int id)
        {
 
            Dictionary<int, CartItemViewModel> shoppingCart = null;

            var sessionCart = HttpContext.Session.GetString("cart");

            //Check to see if the session object exists
            //If not, instantiate the new local collection
            if (String.IsNullOrEmpty(sessionCart))
            {
                //If the session didn't exist yet, instantiate the collection as a new object
                shoppingCart = new Dictionary<int, CartItemViewModel>();
            }
            else
            {
                //Cart already exists - transfer the existing cart items from session into our local shoppingCart
                //DeserializeObject() is a method that converts JSON to C# - we MUST tell this method which C# class
                //to convert the JSON into (in this case, Dictionary<int, CIVM>)
                shoppingCart = JsonConvert.DeserializeObject<Dictionary<int, CartItemViewModel>>(sessionCart);
            }

            //Add newly selected products to the cart
            //Retrieve product from the DB
            Product product = _context.Products.Find(id);

            //Instantiate the CartItemViewModel object so we can add to the cart
            CartItemViewModel civm = new CartItemViewModel(1, product);//add 1 of the selected product to the cart

            //If the product was already in the cart, increase the quantity by 1
            //Else, add the new item to the cart
            if (shoppingCart.ContainsKey(product.ProductId))
            {
                //update qty
                shoppingCart[product.ProductId].Qty++;
            }
            else
            {
                shoppingCart.Add(product.ProductId, civm);
            }

            //Update the session version of the cart
            //Take the local copy (shoppingCart) and serialize it as JSON
            //Then we assign that JSON string as a session value ("cart")
            string jsonCart = JsonConvert.SerializeObject(shoppingCart);
            HttpContext.Session.SetString("cart", jsonCart);

            return RedirectToAction("Index");
        }

        //SHOPPING CART - STEP 12
        //Create the RemoveFromCart action
        public IActionResult RemoveFromCart(int id)
        {
            //retrieve the cart from session
            var sessionCart = HttpContext.Session.GetString("cart");

            //convert JSON cart to c#
            Dictionary<int, CartItemViewModel> shoppingCart = JsonConvert.DeserializeObject<Dictionary<int, CartItemViewModel>>(sessionCart);

            //remove the cart item
            shoppingCart.Remove(id);

            //If there are no remaining items in the cart, remove it from session
            if (shoppingCart.Count == 0)
            {
                HttpContext.Session.Remove("cart");
            }
            //Otherwise, update the session string with our remaining local cart contents
            else
            {
                string jsonCart = JsonConvert.SerializeObject(shoppingCart);
                HttpContext.Session.SetString("cart", jsonCart);
            }

            return RedirectToAction("Index");
        }

        //SHOPPING CART - STEP 14
        //Write the UpdateCart Action
        public IActionResult UpdateCart(int productId, int qty)
        {
            //retrieve the cart from session storage
            var sessionCart = HttpContext.Session.GetString("cart");

            //Deserialize from JSON to C#
            Dictionary<int, CartItemViewModel> shoppingCart = JsonConvert.DeserializeObject<Dictionary<int, CartItemViewModel>>(sessionCart);

            //update the qty for our specific dictionary key
            shoppingCart[productId].Qty = qty;

            //Update the JSON string stored in session with the new qty, then return user to Index action
            string jsonCart = JsonConvert.SerializeObject(shoppingCart);
            HttpContext.Session.SetString("cart", jsonCart);
            return RedirectToAction("Index");
        }

        //SHOPPING CART - STEP 18
        //Write the SubmitOrder Action below
        public async Task<IActionResult> SubmitOrder()
        {
            #region Planning out Order Submission
            //BIG PICTURE PLAN
            //Create Order object -> then save to the DB
            //  - OrderDate
            //  - UserId
            //  - ShipToName, ShipCity, ShipState, ShipZip --> this info needs to be pulled from the UserDetails record
            //Add the record to _context
            //Save DB changes

            //Create OrderProducts object for each item in the cart
            //  - ProductId -> available from cart
            //  - OrderId -> from Order object
            //  - Qty -> available from cart
            //  - ProductPrice -> available from cart
            //Add the record to _context
            //Save DB changes
            #endregion

            //Retrieve current user's ID
            //This is a mechanism provided by Identity to retrieve the User ID in the current HttpContext.
            //If you need to retrieve the ID in ANY Controller, you can use this.
            string? userId = (await _userManager.GetUserAsync(HttpContext.User))?.Id;

            //Retrieve the UserDetails record
            UserDetail ud = _context.UserDetails.Find(userId);

            //Create the Order object and assign the values
            Order o = new()
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                ShipToName = ud.FirstName + " " + ud.LastName,
                ShipVillage = ud.Village
            };

            //Add the Order to our _context
            _context.Orders.Add(o);

            //Retrieve the sessionCart
            var sessionCart = HttpContext.Session.GetString("cart");
            Dictionary<int, CartItemViewModel> shoppingCart = JsonConvert.DeserializeObject<Dictionary<int, CartItemViewModel>>(sessionCart);

            //Create OrderProduct object for each item in the cart
            foreach (var item in shoppingCart)
            {
                OrderProduct op = new OrderProduct()
                {
                    OrderId = o.OrderId,
                    ProductId = item.Key,
                    ProductPrice = item.Value.Product.ProductPrice,
                    Quantity = (short)item.Value.Qty
                };

                o.OrderProducts.Add(op);
            }

            //Commit our changes to the DB
            _context.SaveChanges();
            return RedirectToAction("Index", "Orders");
        }
    }
}
