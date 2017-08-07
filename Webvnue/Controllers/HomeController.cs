using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Webvnue.Controllers
{

    public class HomeController : Controller
    {
        private UserManager<Models.MyIdentityUser> userManager;

        public HomeController()
        {
            Models.MyIdentityDbContext db = new Models.MyIdentityDbContext();

            UserStore<Models.MyIdentityUser> userStore = new UserStore<Models.MyIdentityUser>(db);
            userManager = new UserManager<Models.MyIdentityUser>(userStore);
        }

        // GET: Home
        public ActionResult Index()
        {
            Models.MyIdentityUser user = getCurrentUser();

            if (user != null)
            {
                ViewData["CurrentUser"] = user;

                List<Models.Post> userPosts = getUserPosts(user);
                foreach(var post in userPosts)
                {
                    post.UserModel = getUser(post.UserId);
                    foreach (var comment in post.Comments)
                    {
                        comment.OriginalUser = getUser(comment.OriginalUserId);
                    }

                    post.Comments.Sort((x,y) => DateTime.Compare(x.TimeStamp, y.TimeStamp));
                }
                userPosts.Sort((x, y) => DateTime.Compare(y.TimeStamp, x.TimeStamp));

                ViewData["Posts"] = userPosts;
            }

            return View();
        }

        [HttpPost]
        public ActionResult Index(Models.Register registerModel, string Token)
        {
            if (ModelState.IsValid)
            {
                Models.MyIdentityUser user = new Models.MyIdentityUser();

                user.UserName = registerModel.UserName;
                user.Email = registerModel.Email;
                user.FirstName = registerModel.FirstName;
                user.LastName = registerModel.LastName;
                user.BirthDate = registerModel.BirthDate;

                userManager.UserValidator = new UserValidator<Models.MyIdentityUser>(userManager)
                {
                    RequireUniqueEmail = true
                };

                IdentityResult result = userManager.Create(user, registerModel.Password);

                if (result.Succeeded)
                {
                    //userManager.AddToRole(user.Id, "User");

                    if (Token != null && validateToken(Token))
                    {
                        addNewReferral(user, Token);
                        sendEmail(userManager.FindById(Token), "Webvnue Referral Notification", string.Format("Dear {0}, <br/><br/> {1} has signed up under your referral! <br/><br/> Your monthly income has increased by $4.50. <br/><br/> Best Regards, <br/>Team Webvnue", userManager.FindById(Token).FirstName, user.FirstName));
                    }

                    //addDefaultPicture(user);
                    addUserDefaultProfileBio(user);
                    //initializeUserPosts(user);
                    sendEmail(user, "Webvnue Registration", string.Format("Dear, {0} <br/><br/> Thank you for joining Webvnue. <br/><br/> You're on your way to becoming your own boss. <br/><br/> Best Regards, <br/>Team Webvnue", user.FirstName));

                    IAuthenticationManager authenticationManager = HttpContext.GetOwinContext().Authentication;
                    authenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                    ClaimsIdentity identity = userManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);

                    AuthenticationProperties props = new AuthenticationProperties();

                    authenticationManager.SignIn(props, identity);

                    return Redirect(Url.Content(string.Format("~/{0}", user.UserName)));

                }
                else
                {
                    ModelState.AddModelError("UserName", "Username already exists");
                    ModelState.AddModelError("Email", "Email already exists");
                }
            }
            if (Token != null && validateToken(Token))
            {
                ViewData["Token"] = Token;
            }
            else
            {
                ViewData["Token"] = "";
            }
            return View(registerModel);
        }

        [Route("{user}")]
        public ActionResult Personal(string user)
        {
            Models.MyIdentityUser userLoggedIn = getCurrentUser();

            if (userLoggedIn != null)
            {
                ViewData["CurrentUser"] = userLoggedIn;
            }

            Models.MyIdentityUser requestedUserPage = findUser(user);

            if (requestedUserPage != null)
            {
                ViewData["VisitedUser"] = requestedUserPage;
                ViewData["VisitedUserImages"] = getUserImageIdList(requestedUserPage);
                ViewData["VisitedUserImagesCount"] = getUserImageIdList(requestedUserPage).Count;
                ViewData["VisitedUserReferralListCount"] = getReferralList(requestedUserPage).Count;
                ViewData["VisitedUserProfileBio"] = getUserProfileBio(requestedUserPage.Id);
                ViewData["FollowingCount"] = getUserFollowingCount(requestedUserPage.Id);
                ViewData["FollowerCount"] = getUserFollowerCount(requestedUserPage.Id);

                ViewData["CurrentUserIsFollowing"] = (userLoggedIn != null) ? checkIfFollowing(userLoggedIn.Id, requestedUserPage.Id) : false;

                return View();
            }
            else
            {
                return HttpNotFound("NOT FOUND MOTHER FUCKER");
            }
        }

        [HttpPost]
        public ActionResult UploadProfileImage(HttpPostedFileBase[] uploadImage)
        {
            var db = new Models.MyIdentityDbContext();
            var currentUser = getCurrentUser();

            if (uploadImage.Length == 0)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }

            if (uploadImage[0] == null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }

            if (db.UserProfileImages.Find(currentUser.Id) != null)
            {
                var profile = db.UserProfileImages.Find(currentUser.Id);
                db.UserProfileImages.Remove(profile);
                db.SaveChanges();
            }

            foreach (var image in uploadImage)
            {

                if (image.ContentLength > 0)
                {
                    byte[] imageData = null;
                    using (var binaryReader = new BinaryReader(image.InputStream))
                    {
                        imageData = binaryReader.ReadBytes(image.ContentLength);
                    }
                    var userImage = new Models.UserProfileImage()
                    {
                        UserId = currentUser.Id,
                        ImageData = imageData,
                        FileName = image.FileName
                    };

                    //AddNewPostToFollowers(imageData, currentUser);
                    //AddNewPostToCurrentUser(imageData, currentUser);
                    AddNewPost(imageData, currentUser);

                    db.UserProfileImages.Add(userImage);
                    db.SaveChanges();
                }
            }
            return Redirect(Request.UrlReferrer.ToString());
        }

        [HttpPost]
        public ActionResult UploadImage(HttpPostedFileBase[] uploadMainImage)
        {
            var db = new Models.MyIdentityDbContext();
            var currentUser = getCurrentUser();

            if (uploadMainImage.Length == 0)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }

            if (uploadMainImage[0] == null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }

            foreach (var image in uploadMainImage)
            {

                if (image.ContentLength > 0)
                {
                    byte[] imageData = null;
                    using (var binaryReader = new BinaryReader(image.InputStream))
                    {
                        imageData = binaryReader.ReadBytes(image.ContentLength);
                    }
                    var userImage = new Models.UserImage()
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = currentUser.Id,
                        ImageData = imageData,
                        FileName = image.FileName,
                        Rating = 0,
                        TimeStamp = DateTime.Now,
                        Views = 0

                    };

                    //AddNewPostToFollowers(imageData, currentUser);
                    //AddNewPostToCurrentUser(imageData, currentUser);
                    AddNewPost(imageData, currentUser);

                    db.UserImages.Add(userImage);
                    db.SaveChanges();
                }
            }

            return Redirect(Request.UrlReferrer.ToString());
        }

        [HttpPost]
        public ActionResult ajaxUserProfileBio(string id)
        {

            var db = new Models.MyIdentityDbContext();

            Models.UserProfileBio bio = db.UserProfileBio.FirstOrDefault(x => x.UserID == id);

            //Models.UserProfileBio bio = new Models.UserProfileBio();

            /*
            foreach (var obj in db.UserProfileBio)
            {
                if (obj.UserID == id)
                {
                    bio = obj;
                }
            }
            */

            return Json(new
            {
                Bio = bio
            });
        }

        [HttpPost]
        public ActionResult saveBio(string id, string AboutMe, string Location, string Gender, string Quote)
        {
            updateUserBio(id, AboutMe, Location, Gender, Quote);

            return null;
        }

        [HttpPost]
        public ActionResult addFollowing(string id)
        {
            addNewFollow(getCurrentUser().Id, id);

            return null;
        }

        [HttpPost]
        public ActionResult removeFollowing(string id)
        {
            removeFollow(getCurrentUser().Id, id);

            return null;
        }

        [HttpPost]
        public ActionResult addComment(string id, string PostId, string Message)
        {
            addNewCommentToPost(id, PostId, Message);

            return null;
        }

        public ActionResult profileimg(string id)
        {
            var db = new Models.MyIdentityDbContext();

            var item = db.UserProfileImages.Find(id);
            byte[] buffer = item.ImageData;

            return File(buffer, "image/jpg", string.Format("{0}.jpg", id));
        }

        public ActionResult showImage(string id)
        {
            var db = new Models.MyIdentityDbContext();

            var image = db.UserImages.Find(id);
            byte[] buffer = image.ImageData;

            return File(buffer, "image/jpg", string.Format("{0}.jpg", id));
        }

        
        public ActionResult showPostImage(string id)
        {
            var db = new Models.MyIdentityDbContext();

            byte[] buffer = db.Posts.Find(id).ImageData;

            return File(buffer, "image/jpg", string.Format("{0}.jpg", Guid.NewGuid().ToString()));
        }
        

        public ActionResult photo(string id)
        {
            ViewData["ImageId"] = id;
            return View();
        }

        public ActionResult deletephoto(string id)
        {
            var db = new Models.MyIdentityDbContext();

            /*
            foreach (var obj in db.UserImages)
            {
                if (obj.Id == id)
                {
                    db.UserImages.Remove(obj);
                }
            }
            */

            db.UserImages.Remove(db.UserImages.Find(id));

            db.SaveChanges();

            return Redirect(Request.UrlReferrer.ToString());
        }

        [HttpPost]
        public ActionResult DeletePost(string PostId)
        {
            deletePost(PostId);

            return null;
        }

        [HttpPost]
        public ActionResult CheckIfCommentsUpdated(string PostId, DateTime TimeStamp)
        {
            bool result;

            
            if (checkIfCommentsUpdated(PostId, TimeStamp))
            {
                result = true;
            }
            else
            {
                result = false;
            }
            
            return Json(new
            {
                Result = result
            });
        }

        private Models.UserProfileBio getUserProfileBio(string id)
        {
            var db = new Models.MyIdentityDbContext();

            //Models.UserProfileBio bio = new Models.UserProfileBio();

            /*
            foreach (var obj in db.UserProfileBio)
            {
                if (obj.UserID == id)
                {
                    bio = obj;
                }
            }*/

            Models.UserProfileBio bio = db.UserProfileBio.FirstOrDefault(x => x.UserID == id);

            return bio;

            /*
            return Json(new
            {
                Bio = bio
            });
            */
        }

        private void updateUserBio(string id, string AboutMe, string Location, string Gender, string Quote)
        {
            var db = new Models.MyIdentityDbContext();

            /*
            foreach (var obj in db.UserProfileBio)
            {
                if (obj.UserID == id)
                {
                    obj.AboutMe = AboutMe;
                    obj.Location = Location;
                    obj.Gender = Gender;
                    obj.Quote = Quote;
                }
            }
            */

            db.UserProfileBio.FirstOrDefault(x => x.UserID == id).AboutMe = AboutMe;
            db.UserProfileBio.FirstOrDefault(x => x.UserID == id).Location = Location;
            db.UserProfileBio.FirstOrDefault(x => x.UserID == id).Gender = Gender;
            db.UserProfileBio.FirstOrDefault(x => x.UserID == id).Quote = Quote;

            db.SaveChanges();
        }

        private List<string> getUserImageIdList(Models.MyIdentityUser user)
        {
            var db = new Models.MyIdentityDbContext();

            //List<string> userImageIdList = new List<string>();

            /*
            foreach (var obj in db.UserImages)
            {
                if (obj.UserId == user.Id)
                {
                    userImageIdList.Add(obj.Id);
                }
            }
            */

            List<string> userImageIdList = new List<string>();

            foreach(var image in db.UserImages.Where(x => x.UserId == user.Id).ToList())
            {
                userImageIdList.Add(image.Id);
            }

            return userImageIdList;

        }

        private Models.MyIdentityUser getCurrentUser()
        {
            Models.MyIdentityDbContext db = new Models.MyIdentityDbContext();
            UserStore<Models.MyIdentityUser> userStore = new UserStore<Models.MyIdentityUser>(db);
            UserManager<Models.MyIdentityUser> userManager = new UserManager<Models.MyIdentityUser>(userStore);

            Models.MyIdentityUser user = userManager.FindByName(HttpContext.User.Identity.Name);

            return user;
        }

        private Models.MyIdentityUser findUser(string userName)
        {
            Models.MyIdentityDbContext db = new Models.MyIdentityDbContext();
            UserStore<Models.MyIdentityUser> userStore = new UserStore<Models.MyIdentityUser>(db);
            UserManager<Models.MyIdentityUser> userManager = new UserManager<Models.MyIdentityUser>(userStore);

            Models.MyIdentityUser user = userManager.FindByName(userName);

            return user;
        }

        private List<Models.MyIdentityUser> getReferralList(Models.MyIdentityUser user)
        {
            List<Models.MyIdentityUser> referralList = new List<Models.MyIdentityUser>();

            var db = new Models.MyIdentityDbContext();

            /*
            foreach (var referral in db.Referrals)
            {
                if (user.Id == referral.ReferrerId)
                {
                    referralList.Add(userManager.FindById(referral.RefereeId));
                }
            }
            */

            foreach(var referral in db.Referrals.Where(x => x.ReferrerId == user.Id))
            {
                referralList.Add(userManager.FindById(referral.RefereeId));
            }

            return referralList;
        }

        private bool validateToken(string Token)
        {
            Models.MyIdentityUser user = userManager.FindById(Token);

            if (user != null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private void addNewReferral(Models.MyIdentityUser user, string Token)
        {
            var db = new Models.MyIdentityDbContext();

            Models.Referral newReferral = new Models.Referral();
            newReferral.Id = Guid.NewGuid().ToString();
            newReferral.ReferrerId = userManager.FindById(Token).Id;
            newReferral.RefereeId = user.Id;

            db.Referrals.Add(newReferral);
            db.SaveChanges();
        }

        private void addNewFollow(string userId, string followingUserId)
        {
            var db = new Models.MyIdentityDbContext();

            Models.Follows newFollower = new Models.Follows();
            newFollower.Id = Guid.NewGuid().ToString();
            newFollower.UserId = userId;
            newFollower.FollowingUserId = followingUserId;

            db.UserFollowers.Add(newFollower);
            db.SaveChanges();
        }

        private void removeFollow(string currentUserId, string followingUserId)
        {
            var db = new Models.MyIdentityDbContext();

            /*
            foreach (var obj in db.UserFollowers)
            {
                if (obj.UserId == currentUserId && obj.FollowingUserId == followingUserId)
                {
                    db.UserFollowers.Remove(obj);
                }
            }
            */

            db.UserFollowers.Remove(db.UserFollowers.FirstOrDefault(x => x.UserId == currentUserId && x.FollowingUserId == followingUserId));

            db.SaveChanges();
        }

        private void addUserDefaultProfileBio(Models.MyIdentityUser user)
        {
            var db = new Models.MyIdentityDbContext();

            Models.UserProfileBio bio = new Models.UserProfileBio();

            bio.Id = Guid.NewGuid().ToString();
            bio.UserID = user.Id;
            bio.AboutMe = string.Format("Hello World!, I'm {0}. Let's make some money!", user.FirstName);
            bio.Location = "Webvnue City";
            bio.Gender = "Human";
            bio.Quote = "\"You only live once, but if you do it right, once is enough.\" ― Mae West";

            db.UserProfileBio.Add(bio);
            db.SaveChanges();
        }

        private void sendEmail(Models.MyIdentityUser user, string subject, string body)
        {
            System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(new System.Net.Mail.MailAddress("webvnue@gmail.com", "Webvnue"), new System.Net.Mail.MailAddress(user.Email));
            m.Subject = subject;
            m.Body = body;
            m.IsBodyHtml = true;
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new System.Net.NetworkCredential("webvnue@gmail.com", "Password999");
            smtp.EnableSsl = true;
            smtp.Send(m);
        }

        /*
        private void fillMissingPics()
        {
            var db = new Models.MyIdentityDbContext();

            Models.UserProfileImage defaultImg = db.UserProfileImages.Find("164eb47a-ac68-4e92-8baa-56dd5f730a80");

            foreach (var user in db.Users)
            {
                if (db.UserProfileImages.Find(user.Id) == null)
                {
                    db.UserProfileImages.Add(new Models.UserProfileImage()
                    {
                        UserId = user.Id,
                        ImageData = defaultImg.ImageData,
                        FileName = "Default Image"
                    });
                }
            }

            db.SaveChanges();
        }
        */

        private void addDefaultPicture(Models.MyIdentityUser user)
        {
            var db = new Models.MyIdentityDbContext();
            Models.UserProfileImage defaultImg = db.UserProfileImages.Find("164eb47a-ac68-4e92-8baa-56dd5f730a80");
            db.UserProfileImages.Add(new Models.UserProfileImage()
            {
                UserId = user.Id,
                ImageData = defaultImg.ImageData,
                FileName = "Default Image"
            });
            db.SaveChanges();
        }

        /*
        private void initializeUserPosts(Models.MyIdentityUser user)
        {
            var db = new Models.MyIdentityDbContext();
            db.UserPosts.Add(new Models.UserPosts()
            {
                UserId = user.Id
                //Posts = new Collection<Models.Post>()
            });

            db.SaveChanges();
        }
        */

        private bool checkIfFollowing(string currentUserId, string visitedUserId)
        {
            var db = new Models.MyIdentityDbContext();

            /*
            foreach(var obj in db.UserFollowers)
            {
                if(obj.UserId == currentUserId && obj.FollowingUserId == visitedUserId)
                {
                    return true;
                }
            }
            */

            if (db.UserFollowers.FirstOrDefault(x => x.UserId == currentUserId && x.FollowingUserId == visitedUserId) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int getUserFollowingCount(string userId)
        {
            var db = new Models.MyIdentityDbContext();

            /*
            int count = 0;

            foreach(var obj in db.UserFollowers)
            {
                if(obj.UserId == userId)
                {
                    count++;
                }
            }
            */

            int count = db.UserFollowers.Where(x => x.UserId == userId).Count();

            return count;
        }

        private int getUserFollowerCount(string userId)
        {
            var db = new Models.MyIdentityDbContext();

            /*
            int count = 0;

            foreach (var obj in db.UserFollowers)
            {
                if (obj.FollowingUserId == userId)
                {
                    count++;
                }
            }
            */

            int count = db.UserFollowers.Where(x => x.FollowingUserId == userId).Count();

            return count;
        }

        private List<Models.MyIdentityUser> getFollowers(string userId)
        {
            var db = new Models.MyIdentityDbContext();

            //List<Models.MyIdentityUser> followerList = new List<Models.MyIdentityUser>();

            /*
            foreach (var obj in db.UserFollowers)
            {
                if (obj.FollowingUserId == userId)
                {
                    followerList.Add(userManager.FindById(obj.UserId));
                }
            }
            */

            List<Models.MyIdentityUser> followerList = new List<Models.MyIdentityUser>();

            foreach(var follower in db.UserFollowers.Where(x => x.FollowingUserId == userId))
            {
                followerList.Add(userManager.FindById(follower.UserId));
            }

            return followerList;
        }

        private List<Models.MyIdentityUser> getFollowing(string userId)
        {
            var db = new Models.MyIdentityDbContext();

            List<Models.MyIdentityUser> followingList = new List<Models.MyIdentityUser>();

            foreach (var following in db.UserFollowers.Where(x => x.UserId == userId))
            {
                followingList.Add(userManager.FindById(following.FollowingUserId));
            }

            return followingList;
        }


        private List<Models.Post> getUserPosts(Models.MyIdentityUser user)
        {
            var db = new Models.MyIdentityDbContext();

            List<Models.Post> userPostList = new List<Models.Post>();


            foreach(var following in getFollowing(user.Id))
            {
                userPostList.AddRange(db.Posts.Where(x => x.UserId == following.Id).ToList());
            }
            userPostList.AddRange(db.Posts.Where(x => x.UserId == user.Id).ToList());

            return userPostList;
        }
        

        private Models.MyIdentityUser getUser(string userId)
        {
            var db = new Models.MyIdentityDbContext();

            return db.Users.Find(userId);
        }

        private void addNewCommentToPost(string id, string postId, string message)
        {
            var db = new Models.MyIdentityDbContext();

            var newComment = new Models.Comment() {
                Id = Guid.NewGuid().ToString(),
                OriginalUserId = id,
                Message = message,
                TimeStamp = DateTime.Now
            };

            db.Posts.FirstOrDefault(x => x.Id == postId).Comments.Add(newComment);

            db.SaveChanges();
        }

        /*
        private void AddNewPostToFollowers(byte[] imageData, Models.MyIdentityUser user)
        {
            var db = new Models.MyIdentityDbContext();

            var post = new Models.Post()
            {
                Id = Guid.NewGuid().ToString(),
                OriginalPostUserId = user.Id,
                ImageData = imageData,
                TimeStamp = DateTime.Now
            };

            foreach (var follower in getFollowers(user.Id))
            {
                db.UserPosts.Find(follower.Id).Posts.Add(post);
            }

            db.SaveChanges();
        }

        private void AddNewPostToCurrentUser(byte[] imageData, Models.MyIdentityUser user)
        {
            var db = new Models.MyIdentityDbContext();

            var post = new Models.Post()
            {
                Id = Guid.NewGuid().ToString(),
                OriginalPostUserId = user.Id,
                ImageData = imageData,
                TimeStamp = DateTime.Now
            };

            db.UserPosts.Find(user.Id).Posts.Add(post);

            db.SaveChanges();
        }
        */

        private void AddNewPost(byte[] imageData, Models.MyIdentityUser user)
        {
            var db = new Models.MyIdentityDbContext();

            var post = new Models.Post()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                ImageData = imageData,
                TimeStamp = DateTime.Now
            };

            db.Posts.Add(post);

            db.SaveChanges();
        }

        private void deletePost(string postId)
        {
            var db = new Models.MyIdentityDbContext();

            db.Posts.Remove(db.Posts.FirstOrDefault(x => x.Id == postId));

            db.SaveChanges();
        }

        private bool checkIfCommentsUpdated(string postid, DateTime timeStamp)
        {
            var db = new Models.MyIdentityDbContext();

            Models.Post postToCheck = db.Posts.FirstOrDefault(x => x.Id == postid);

            DateTime latestTimeStamp = postToCheck.Comments.Max(x => x.TimeStamp);


            if (latestTimeStamp.Date.Equals(postToCheck.TimeStamp.Date))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
