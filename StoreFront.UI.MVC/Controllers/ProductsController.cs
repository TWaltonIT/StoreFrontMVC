using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StoreFront.DATA.EF.Models;
using Microsoft.AspNetCore.Authorization;
using StoreFront.UI.MVC.Utilities;
using System.Drawing;
using X.PagedList;

namespace StoreFront.UI.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly KonohaExpressContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(KonohaExpressContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Products
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var products =
                _context.Products.Where(p => !p.IsDiscontinued)//SELECT * FROM PRODUCTS WHERE IsDiscontinued != true
                .Include(p => p.Category)//Similar to a JOIN on the Category table
                .Include(p => p.Supplier)//Similar to a JOIN on the Supplier table
                .Include(p => p.OrderProducts)//Similar to a JOIN on the OrderProducts table
                .Include(p => p.Nature)
                .Include(p => p.ProductStatus);
            

            return View(await products.ToListAsync());
        }

        [AllowAnonymous]
        public async Task<IActionResult> TiledProducts(string searchTerm, int categoryId = 0, int page = 1)
        {

            int pageSize = 6;

            var products = _context.Products.Where(p => !p.IsDiscontinued)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.OrderProducts)
                .Include(p => p.Nature)
                .Include(p => p.ProductStatus).ToList();

            //SERVER SIDE FILTERING - STEP 08
            #region Optional Category Filter

            //Create a ViewData[] object to send a list of categories to the View
            //This is similar to what gets scaffolded in Products/Create()
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            ViewBag.Category = 0;

            //At this point, we need to add int categoryId as a parameter to the TiledProducts action
            if (categoryId != 0)
            {
                products = products.Where(p => p.CategoryId == categoryId).ToList();
                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", categoryId);
                ViewBag.Category = categoryId;
            }


            #endregion

            //SERVER SIDE FILTERING - STEP 07
            #region Optional Search Filter

            if (!String.IsNullOrEmpty(searchTerm))
            {
                //In these scopes, there *is* a search term
                products = products.Where(p =>
                p.ProductName.ToLower().Contains(searchTerm.ToLower()) ||
                p.Category.CategoryName.ToLower().Contains(searchTerm.ToLower()) ||
                p.Supplier.SupplierName.ToLower().Contains(searchTerm.ToLower()) ||
                p.ProductDescription.ToLower().Contains(searchTerm.ToLower())).ToList();

                //ViewBag for total # of results
                ViewBag.NbrResults = products.Count;
                //ViewBag to repeat the user's search term back to them
                ViewBag.SearchTerm = searchTerm;
            }
            else
            {
                ViewBag.NbrResults = null;
                ViewBag.SearchTerm = null;
            }

            #endregion

            return View( products.ToPagedList(page , pageSize));
        }

        [AcceptVerbs("POST")]
        
        public JsonResult AjaxDelete(int id)
        {
            Product product = _context.Products.Find(id);
            _context.Products.Remove(product);
            _context.SaveChanges();

            string confirmMessage = $"Deleted the product {product.ProductName} from the database!";
            return Json(new { id = id, message = confirmMessage });
        }

        [AllowAnonymous]
        public PartialViewResult ProductDetails(int id)
        {
            var product = _context.Products.Find(id);
            return PartialView(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            ViewData["NatureId"] = new SelectList(_context.ChakraNatures, "NatureId", "NatureName");
            ViewData["ProductStatusId"] = new SelectList(_context.ProductStatuses, "ProductStatusId", "ProductStatusName");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,ProductPrice,ProductDescription,ItemsInStock,ItemsOnOrder,CategoryId,SupplierId,IsDiscontinued,ProductImage,NatureId,ProductStatusId,Image")] Product product)
        {
            if (ModelState.IsValid)
            {
                // IMAGE UPLOAD - STEP 11
                #region File Upload - Create

                // Check to see if a file was uploaded
                if (product.Image != null)
                {
                    // Check the filetype
                    // retrieve the extension of the uploaded file
                    string ext = Path.GetExtension(product.Image.FileName);

                    // Create a list of valid extensions to check against
                    string[] validExts = { ".jpg", ".jpeg", ".gif", ".png" };

                    // Verify the uploaded file has an extenstion matching one of the extensions in the list above
                    // AND verify the file size will work within our .NET app.
                    if (validExts.Contains(ext.ToLower()) && product.Image.Length < 4_194_303)// Underscores make the int more readable
                    {
                        // Generate a unique filename
                        product.ProductImage = Guid.NewGuid() + ext;

                        // Save the file to our web server (our wwwroot/images folder)
                        // To access wwwroot, add a property to the controller for the _webHostEnvironment
                        // (See the top of this Controller for the example)
                        // Retrieve the path to wwwroot
                        string webRootPath = _webHostEnvironment.WebRootPath;
                        // Make a variable for the full image path
                        string fullImagePath = webRootPath + "/img/";

                        // Create a MemoryStream to read the image into the server memory
                        using (var memoryStream = new MemoryStream())
                        {
                            await product.Image.CopyToAsync(memoryStream);// Transfer file from the POST action to server memory
                            using (var img = Image.FromStream(memoryStream))// relies on using statement for System.Drawing 
                            {
                                // Now we send the Image to ImageUtility for resizing and thumbnail creation
                                // We need 5 parameters for the ImageUtility.RResize():
                                //1) (int) maximum image size
                                //2) (int) maximum thumbnail size
                                //3) (string) full path where file will be saved
                                //4) (Image) the actual Image file
                                //5) (string) filename
                                int maxImageSize = 500;//size is set in pixels
                                int maxThumbSize = 100;

                                // Below relies on the using statement for our UI layer .Utilities
                                ImageUtility.ResizeImage(fullImagePath, product.ProductImage, img, maxImageSize, maxThumbSize);
                                //myFile.Save("path/to/folder/", "fileName"); - How we save a NON-image
                            }
                        }
                    }
                }
                else
                {
                    // If no image was uploaded, assign a default filename
                    // We also need to add a default image and name it "noimage.png" then copy it to our wwwroot/images folder
                    product.ProductImage = "noimage.png";
                }
                // IMAGE UPLOAD - STEP 12
                // Add noimage.png to our images folder
                #endregion
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["NatureId"] = new SelectList(_context.ChakraNatures, "NatureId", "NatureName", product.NatureId);
            ViewData["ProductStatusId"] = new SelectList(_context.ProductStatuses, "ProductStatusId", "ProductStatusName", product.ProductStatusId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName", product.SupplierId);
            return View(product);
        }

        // GET: Products/Edit/5
        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["NatureId"] = new SelectList(_context.ChakraNatures, "NatureId", "NatureName", product.NatureId);
            ViewData["ProductStatusId"] = new SelectList(_context.ProductStatuses, "ProductStatusId", "ProductStatusName", product.ProductStatusId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName", product.SupplierId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,ProductPrice,ProductDescription,ItemsInStock,ItemsOnOrder,CategoryId,SupplierId,IsDiscontinued,ProductImage,NatureId,ProductStatusId, Image")] Product product)
        {

            // IMAGE UPLOAD - STEP 18
            #region File Upload - Edit

            //Retain the old image filename so we can delete if a new file is uploaded
            string oldImageName = product.ProductImage;

            //Check if the user uploaded a file
            if (product.Image != null)
            {
                //Get the file's extension
                string ext = Path.GetExtension(product.Image.FileName);

                //list valid extensions
                string[] validExts = { ".jpg", ".jpeg", ".gif", ".png" };

                //check the file's extension against the list of valid extensions
                if (validExts.Contains(ext.ToLower()) && product.Image.Length < 4_194_303)
                {
                    //generate a new unique filename
                    product.ProductImage = Guid.NewGuid() + ext;
                    //put together our file path to save the image
                    string webRootPath = _webHostEnvironment.WebRootPath;
                    string fullPath = webRootPath + "/img/";

                    //delete the old image if one existed
                    if (oldImageName != "noimage.png")
                    {
                        ImageUtility.Delete(fullPath, oldImageName);
                    }

                    //Save the new image to our server (wwwroot/img/)
                    using (var memoryStream = new MemoryStream())
                    {
                        await product.Image.CopyToAsync(memoryStream);
                        using (var img = Image.FromStream(memoryStream))
                        {
                            int maxImageSize = 500;
                            int maxThumbSize = 100;
                            ImageUtility.ResizeImage(fullPath, product.ProductImage, img, maxImageSize, maxThumbSize);
                        }
                    }
                }
            }

            #endregion

            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["NatureId"] = new SelectList(_context.ChakraNatures, "NatureId", "NatureName", product.NatureId);
            ViewData["ProductStatusId"] = new SelectList(_context.ProductStatuses, "ProductStatusId", "ProductStatusName", product.ProductStatusId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName", product.SupplierId);
            return View(product);
        }

        //// GET: Products/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.Products == null)
        //    {
        //        return NotFound();
        //    }

        //    var product = await _context.Products
        //        .Include(p => p.Category)
        //        .Include(p => p.Nature)
        //        .Include(p => p.ProductStatus)
        //        .Include(p => p.Supplier)
        //        .FirstOrDefaultAsync(m => m.ProductId == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(product);
        //}

        //// POST: Products/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.Products == null)
        //    {
        //        return Problem("Entity set 'KonohaExpressContext.Products'  is null.");
        //    }
        //    var product = await _context.Products.FindAsync(id);
        //    if (product != null)
        //    {
        //        _context.Products.Remove(product);
        //    }
            
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool ProductExists(int id)
        {
          return (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }
    }
}
