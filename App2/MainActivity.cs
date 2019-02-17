using System;
using System.IO;
using System.Linq;
using Android;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using App2.Component;
using App2.Models;
using App2.Repository;
using Realms;
using static Android.App.ActionBar;
using Environment = Android.OS.Environment;
using Orientation = Android.Widget.Orientation;
using Path = System.IO.Path;

namespace App2
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private CarRepository _carRepository;
        private Bitmap bitmap;

        private WinSelectDialog _winDialog;
        private WinSelectDialog _searchDialog;
        private WinSelectDialog _editDialog;

        private Car _activeCar;
        private CarImage _selectetImage;
        private string[] _options;

        public static Java.IO.File _file;
        public static Java.IO.File _dir;
        public static Bitmap _bitmap;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _carRepository = new CarRepository();
            _options = Resources.GetStringArray(Resource.Array.menu);
            Array.Sort(_options);

            SetContentView(Resource.Layout.activity_main);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            _dir = new Java.IO.File(
                Environment.GetExternalStoragePublicDirectory(
                    Environment.DirectoryPictures), "CameraAppDemo");
            if (!_dir.Exists())
            {
                _dir.Mkdirs();
            }

        }

        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            //MenuInflater.Inflate(Resource.Menu.menu_mXain, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_delete)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.nav_camera)
            {
                AddFromCamera();
            }
            else if (id == Resource.Id.nav_gallery)
            {
                _searchDialog = new WinSelectDialog(this, "Szukaj");
                _searchDialog.SetButtonTitle("Szukaj");
                _searchDialog.Build(_carRepository.GetAllCar().Select(x => x.WIN).ToArray(), SearchAction, false);
                _searchDialog.Show();
            }
            else if (id == Resource.Id.nav_slideshow)
            {

            }
            else if (id == Resource.Id.nav_manage)
            {

            }
            else if (id == Resource.Id.nav_share)
            {

            }
            else if (id == Resource.Id.nav_send)
            {

            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        private void AddFromCamera()
        {
            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
            StrictMode.SetVmPolicy(builder.Build());

            Intent intent = new Intent(MediaStore.ActionImageCapture);
            _file = new Java.IO.File(_dir, string.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_file));
            StartActivityForResult(intent, 0);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {

            base.OnActivityResult(requestCode, resultCode, data);

            var _imageView = FindViewById<ImageView>(Resource.Id.imageView1);

            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Android.Net.Uri contentUri = Android.Net.Uri.FromFile(_file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);

            // Display in ImageView. We will resize the bitmap to fit the display
            // Loading the full sized image will consume to much memory 
            // and cause the application to crash.

            int height = Resources.DisplayMetrics.HeightPixels;
            int width = _imageView.Height;
            _bitmap = _file.Path.LoadAndResizeBitmap(width, height);
            if (_bitmap != null)
            {
                _imageView.SetImageBitmap(_bitmap);
                _bitmap = null;
            }

            // Dispose of the Java side bitmap.
            GC.Collect();

            //bitmap = (Bitmap)data.Extras.Get("data");
            //var cars = _carRepository.GetAllCar();
            //var wins = cars.Select(x => x.WIN).ToArray();

            //_winDialog = new WinSelectDialog(this, "Dodaj zdjęcie", _activeCar?.WIN);
            //_winDialog.Build(wins, OkAction);
            //_winDialog.Show();
        }

        private void OnEdit(object sender, DialogClickEventArgs e)
        {
            var win = _editDialog.Win.Trim().ToUpper();
            var note = _editDialog.Note;
            var car = _carRepository.GetCarByWin(win);

            _carRepository.EditCarImage(_selectetImage.Id,car.Id,note);
            CreateViewForSingleCar(car.WIN);
        }

        private void OkAction(object sender, DialogClickEventArgs e)
        {
            var win = _winDialog.Win.Trim().ToLower();
            var note = _winDialog.Note;
            var carId = 0;

            var car = _carRepository.GetCarByWin(win);
            if (car == null)
                carId = _carRepository.AddCar(new Car { WIN = win });
            else
                carId = car.Id;

            var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            var z = Directory.CreateDirectory(sdCardPath + "/FotoCar/" + win);
            var filePath = System.IO.Path.Combine(z.FullName, $"{DateTime.Now.TimeOfDay.ToString()}.png");
            var stream = new FileStream(filePath, FileMode.Create);
            bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
            stream.Close();

            _carRepository.AddCarImage(new CarImage
            {
                CarId = carId,
                PhotoName = filePath,
                Note = note
            });

            CreateViewForSingleCar(win);
        }

        private void SearchAction(object sender, DialogClickEventArgs e)
        {
            var win = _searchDialog.Win.Trim().ToLower();
            CreateViewForSingleCar(win);
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            var carImageId = v.Id;
            _selectetImage = _carRepository.GetCarImage(carImageId);
            menu.SetHeaderTitle("Zdjęcie");
            var menuItems = Resources.GetStringArray(Resource.Array.menu);
            for (var i = 0; i < menuItems.Length; i++)
                menu.Add(Menu.None, i, i, menuItems[i]);
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            var menuItemIndex = item.ItemId;
            if (menuItemIndex == 0) //Edytuj
            {
                _editDialog = new WinSelectDialog(this,"Edytuj zdjęcie",_activeCar.WIN,_selectetImage.Note);
                _editDialog.SetButtonTitle("Edytuj");
                _editDialog.Build(_carRepository.GetAllCar().Select(x => x.WIN).ToArray(),OnEdit);
                _editDialog.Show();
            }

            else if (menuItemIndex == 1) // usun
            {
                var directoryName = Path.GetDirectoryName(_selectetImage.PhotoName);
                File.Delete(_selectetImage.PhotoName);
              
                var deleteCar = _carRepository.DeleteImageFromCar(_selectetImage);
                if (deleteCar && directoryName != null)
                {
                    Directory.Delete(directoryName);
                }

                Toast.MakeText(this, "Usunięto", ToastLength.Short).Show();
                CreateViewForSingleCar(_activeCar.WIN);
            }
            else if(menuItemIndex ==2)//wyslij
            {
                
            }
            return true;
        }

        private void CreateViewForSingleCar(string win)
        {
            //    var car = _carRepository.GetCarByWin(win);
            //    _activeCar = car;

            //    var lajt = FindViewById<LinearLayout>(Resource.Id.lajt);
            //    lajt.RemoveAllViews();
            //    if (car == null)
            //        return;

            //    var images = _carRepository.GetCarImages(car.Id);

            //    var Win = new TextView(this);
            //    Win.Text = car.WIN;
            //    Win.SetTextSize(ComplexUnitType.Pt, 11);

            //    lajt.AddView(Win);

            //    foreach (var carImage in images)
            //    {
            //        var note = new TextView(this);
            //        note.SetTextSize(ComplexUnitType.Pt, 8);
            //        note.Text = carImage.Note;
            //        var imageView = new ImageView(this);
            //        imageView.Id = carImage.Id;
            //        imageView.SetMinimumWidth(500);
            //        imageView.SetMinimumHeight(700);
            //        imageView.Click += delegate
            //        {
            //            var intent = new Intent(this, typeof(ImageActivity));
            //            intent.PutExtra("imgUrl", carImage.PhotoName);
            //            StartActivity(intent);
            //        };

            //        RegisterForContextMenu(imageView);

            //        //imageView.LongClick += (sender, e) => { _selectetImage = carImage; };

            //        imageView.SetImageURI(Android.Net.Uri.Parse(carImage.PhotoName));
            //        lajt.AddView(imageView);
            //        lajt.AddView(note);
            //    }
        }
    }
    public static class Extension
    {
        public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
        {
            // First we get the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
           var z = BitmapFactory.DecodeFile(fileName, options);

            // Next we calculate the ratio that we need to resize the image by
            // in order to fit the requested dimensions.
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                    ? outHeight / height
                    : outWidth / width;
            }

            // Now we will load the image and have BitmapFactory resize it for us.
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

            return resizedBitmap;
        }
    }
}

