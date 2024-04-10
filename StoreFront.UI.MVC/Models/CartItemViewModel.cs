#region Steps to Implement Session Based Shopping Cart
/*
 * 1) Create the CartItemViewModel class in [ProjName].UI.MVC/Models folder
 * 2) Register Session in program.cs (builder.Services.AddSession... && app.UseSession())
 * 3) Add the 'Add To Cart' button in the Index and/or Details view of your Products
 * 4) Create the ShoppingCartController (empty controller -> named ShoppingCartController)
 *      - add using statements
 *          - using StoreFront.DATA.EF.Models;
 *          - using Microsoft.AspNetCore.Identity;
 *          - using StoreFront.UI.MVC.Models;
 *          - using Newtonsoft.Json;
 *      - Add props for the StoreFrontContext && UserManager
 *      - Create a constructor for the controller - assign values to context && usermanager
 *      - Code the AddToCart() action
 *      - Code the Index() action
 *      - Code the Index View
 *          - Start with the basic table structure
 *          - Show the items that are easily accessible (like the properties from the model)
 *          - Calculate/show the lineTotal
 *          - Add the RemoveFromCart <a>
 *      - Code the RemoveFromCart() action
 *          - verify the button for RemoveFromCart in the Index view is coded with the controller/action/id
 *      - Add UpdateCart <form> to the Index View
 *      - Code the UpdateCart() action
 *      - Add Submit Order button to Index View
 *      - Code SubmitOrder() action
 * */
#endregion

//SHOPPING CART - STEP 01
//Create this ViewModel by right-clicking the Models folder in the UI Layer:
//Add -> Class, name it CartItemViewModel
//Add using statement for the Data Layer Models folder
using StoreFront.DATA.EF.Models;

namespace StoreFront.UI.MVC.Models
{
    public class CartItemViewModel
    {
        public int Qty { get; set; }

        public Product Product { get; set; }
        //Above is an example of a concept called "Containment"
        //This is a use of a complex datatype as a field/property in a Class.

        //Primitive Datatype: Any Class that stores a single property/value (int, bool, char, decimal, string, etc.)
        //Complex Datatype: Any Class with multiple properties/values (DateTime, Product, ContactViewModel, Console, etc.)

        //Build the Constructor
        public CartItemViewModel(int qty, Product product)
        {
            //Assignment
            Qty = qty;
            Product = product;
        }

    }
}
