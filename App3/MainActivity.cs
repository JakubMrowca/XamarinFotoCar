using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
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
using App3.Components;
using App3.Models;
using App3.Repository;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Realms;
using static Android.App.ActionBar;
using Environment = Android.OS.Environment;
using Orientation = Android.Widget.Orientation;
using Path = System.IO.Path;

namespace App3
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true,ScreenOrientation =ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        TextView textMessage;
        private CarRepository _carRepository;
        private Bitmap bitmap;

        private WinSelectDialog _winDialog;
        private WinSelectDialog _searchDialog;
        private WinSelectDialog _editDialog;
        private WinSelectDialog _addFromGaleryDialog;

        private Car _activeCar;
        private CarImage _selectetImage;
        private string[] _options;

        public static Java.IO.File _file;
        public static Java.IO.File _dir;
        public static Bitmap _bitmap;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _carRepository = new CarRepository();
            _options = Resources.GetStringArray(Resource.Array.menu);
            Array.Sort(_options);

            textMessage = FindViewById<TextView>(Resource.Id.message);
            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);

            _dir = new Java.IO.File(
                Environment.GetExternalStoragePublicDirectory(
                    Environment.DirectoryPictures), "CameraAppTMP");
            if (!_dir.Exists())
            {
                _dir.Mkdirs();
            }

            CreateViewForLastVin();
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.navigation_camera:
                   AddFormCamera();
                    return true;
                case Resource.Id.navigation_search:
                    Search();
                    return true;
                case Resource.Id.navigation_add:
                    AddFormGalery();
                    return true;
            }
            return false;
        }

        private void AddFormCamera()
        {
            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
            StrictMode.SetVmPolicy(builder.Build());

            Intent intent = new Intent(MediaStore.ActionImageCapture);
            _file = new Java.IO.File(_dir, string.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_file));
            StartActivityForResult(intent, 0);
        }

        private async Task AddFormGalery()
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsPickPhotoSupported)
            {
                Toast.MakeText(this, "Brak uprawnień", ToastLength.Short).Show();
                return;
            }
            var file = await CrossMedia.Current.PickPhotoAsync();
            _file = new Java.IO.File(file.Path);
            _addFromGaleryDialog = new WinSelectDialog(this,"Dodaj z galeri",_activeCar?.WIN);
            _addFromGaleryDialog.SetButtonTitle("Dodaj");
            _addFromGaleryDialog.Build(_carRepository.GetAllCar().Select(x => x.WIN).ToArray(), AddFromGaleryOkAction);
            _addFromGaleryDialog.Show();
        }

        private void Search()
        {
            _searchDialog = new WinSelectDialog(this, "Szukaj");
            _searchDialog.SetButtonTitle("Szukaj");
            _searchDialog.Build(_carRepository.GetAllCar().Select(x => x.WIN).ToArray(), SearchAction, false);
            _searchDialog.Show();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {

            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Canceled)
            {
                _file.Delete();
            }
            else
            {
                Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                Android.Net.Uri contentUri = Android.Net.Uri.FromFile(_file);
                mediaScanIntent.SetData(contentUri);
                SendBroadcast(mediaScanIntent);

                GC.Collect();


                var cars = _carRepository.GetAllCar();
                var wins = cars.Select(x => x.WIN).ToArray();

                _winDialog = new WinSelectDialog(this, "Dodaj zdjęcie", _activeCar?.WIN);
                _winDialog.SetDissmisEvent(new OnDismissListener(() =>
                {
                    _file.Delete();
                }));
                _winDialog.Build(wins, OkAction);
                _winDialog.Show();
            }
          
        }

        private void OnEdit(object sender, DialogClickEventArgs e)
        {
            var win = _editDialog.Win.Trim().ToUpper();
            var note = _editDialog.Note;
            var car = _carRepository.GetCarByWin(win);

            _carRepository.EditCarImage(_selectetImage.Id, car.Id, note);
            Toast.MakeText(this, "Z aktualizowano", ToastLength.Short).Show();
            CreateViewForSingleCar(car.WIN);
        }

        private void OkAction(object sender, DialogClickEventArgs e)
        {
            var win = _winDialog.Win.Trim().ToUpper();
            var note = _winDialog.Note;

            CreateCarAndCarImage(win, note);
        }

        private void CreateCarAndCarImage(string win, string note)
        {
            var carId = 0;

            var car = _carRepository.GetCarByWin(win);
            if (car == null)
                carId = _carRepository.AddCar(new Car { WIN = win });
            else
                carId = car.Id;

            var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            var z = Directory.CreateDirectory(sdCardPath + "/FotoCar/" + win);
            var newName = $"{z.FullName}/{DateTime.Now.TimeOfDay.ToString()}.jpg";
            _file.RenameTo(new Java.IO.File(newName));
            _file.Delete();

            _carRepository.AddCarImage(new CarImage
            {
                CarId = carId,
                PhotoName = newName,
                Note = note
            });
            CreateViewForSingleCar(win);
        }

        private void AddFromGaleryOkAction(object sender, DialogClickEventArgs e)
        {
            var win = _addFromGaleryDialog.Win.Trim().ToUpper();
            var note = _addFromGaleryDialog.Note;

            CreateCarAndCarImage(win, note);
        }

        private void SearchAction(object sender, DialogClickEventArgs e)
        {
            var win = _searchDialog.Win.Trim().ToUpper();
            CreateViewForSingleCar(win);
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            var carImageId = v.Id;
            _selectetImage = _carRepository.GetCarImage(carImageId);
            menu.SetHeaderTitle(_selectetImage.CreatedDateTime);
            var menuItems = Resources.GetStringArray(Resource.Array.menu);
            for (var i = 0; i < menuItems.Length; i++)
                menu.Add(Menu.None, i, i, menuItems[i]);
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            var menuItemIndex = item.ItemId;
            if (menuItemIndex == 0) //Edytuj
            {
                _editDialog = new WinSelectDialog(this, "Edytuj zdjęcie", _activeCar.WIN, _selectetImage.Note);
                _editDialog.SetButtonTitle("Edytuj");
                _editDialog.Build(_carRepository.GetAllCar().Select(x => x.WIN).ToArray(), OnEdit);
                _editDialog.Show();
            }

            else if (menuItemIndex == 1) // usun
            {
                var winToDel = _activeCar.WIN;
                var directoryName = System.IO.Path.GetDirectoryName(_selectetImage.PhotoName);
                File.Delete(_selectetImage.PhotoName);

                var deleteCar = _carRepository.DeleteImageFromCar(_selectetImage);
                if (deleteCar && directoryName != null)
                {
                    Directory.Delete($"{directoryName}");
                    Toast.MakeText(this, "Usunięto", ToastLength.Short).Show();
                    GoToHome();
                }
                else
                {
                    Toast.MakeText(this, "Usunięto", ToastLength.Short).Show();
                    CreateViewForSingleCar(_activeCar.WIN);
                }
            }
            else if (menuItemIndex == 2)//wyslij
            {

            }
            return true;
        }

        private void GoToHome()
        {
            var lajt = FindViewById<LinearLayout>(Resource.Id.lajt);
            lajt.RemoveAllViews();
        }

        private void CreateViewForSingleCar(string win)
        {
            var car = _carRepository.GetCarByWin(win);
            _activeCar = car;

            var lajt = FindViewById<LinearLayout>(Resource.Id.lajt);
            lajt.RemoveAllViews();
            if (car == null)
                return;

            var images = _carRepository.GetCarImages(car.Id);

            var Win = new TextView(this);
            Win.Text = car.WIN;
            Win.SetTextSize(ComplexUnitType.Pt, 11);
            Win.SetTextColor(new Color(222, 81,81));
            Win.Gravity = GravityFlags.Center;
            var lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
            lp.SetMargins(0, 10, 0, 10);

            var lpNote = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
            lpNote.SetMargins(0, 10, 0, 0);
            lajt.AddView(Win,lp);

            foreach (var carImage in images)
            {

                var dateInfo = new TextView(this);
                dateInfo.SetTextSize(ComplexUnitType.Pt, 6);
                dateInfo.Text = "Zrobiono";
                dateInfo.SetTextColor(new Color(222, 81, 81));
                dateInfo.Gravity = GravityFlags.Center;
                
                var date = new TextView(this);
                date.SetTextSize(ComplexUnitType.Pt, 8);
                date.Text = carImage.CreatedDateTime;
                date.Gravity = GravityFlags.Center;

                var imageView = new ImageView(this);
                imageView.Id = carImage.Id;
                imageView.SetMinimumWidth(300);
                imageView.SetMinimumHeight(500);
                imageView.SetMaxWidth(400);
                imageView.SetMaxHeight(600);
                imageView.Click += delegate
                {
                    var intent = new Intent(this, typeof(ImageActivity));
                    intent.PutExtra("imgUrl", carImage.PhotoName);
                    StartActivity(intent);
                };

                RegisterForContextMenu(imageView);

             
                int height = 600;
                int width = 400;
                var bitmapCar = carImage.PhotoName.LoadAndResizeBitmap(width, height);
                if (bitmapCar != null)
                {
                    imageView.SetImageBitmap(bitmapCar);
                }

                GC.Collect();

                lajt.AddView(imageView);
                if (!string.IsNullOrEmpty(carImage.Note))
                {
                    var note = new TextView(this);
                    var noteInfo = new TextView(this);
                    noteInfo.SetTextSize(ComplexUnitType.Pt, 6);
                    noteInfo.Text = "Notatka";
                    noteInfo.SetTextColor(new Color(222, 81, 81));
                    noteInfo.Gravity = GravityFlags.Center;

                    note.SetTextSize(ComplexUnitType.Pt, 8);
                    note.Text = $"{carImage.Note}";
                    note.Gravity = GravityFlags.Center;

                    lajt.AddView(noteInfo, lpNote);
                    lajt.AddView(note);
                }
                lajt.AddView(dateInfo,lpNote);
                lajt.AddView(date);
            }
        }

        private void CreateViewForLastVin()
        {
            var cars = _carRepository.GetCarOrdered();

            var lajt = FindViewById<LinearLayout>(Resource.Id.lajt);
            lajt.RemoveAllViews();
            if (cars == null)
                return;

            foreach (var car in cars)
            {
                var images = _carRepository.GetCarImages(car.Id);
                var Win = new TextView(this);
                Win.Text = car.WIN;
                Win.SetTextSize(ComplexUnitType.Pt, 11);
                Win.SetTextColor(new Color(222, 81, 81));
                Win.Gravity = GravityFlags.Center;
                Win.SetBackgroundColor(new Color(Color.WhiteSmoke));
                Win.SetShadowLayer(2,2,2, Color.Black);
                Win.Click += delegate { CreateViewForSingleCar(car.WIN); };

                var lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
                lp.SetMargins(0, 10, 0, 10);
                lajt.AddView(Win, lp);
            }


            //foreach (var carImage in images)
            //{
            //    var note = new TextView(this);
            //    var noteInfo = new TextView(this);
            //    noteInfo.SetTextSize(ComplexUnitType.Pt, 6);
            //    noteInfo.Text = "Notatka";
            //    noteInfo.SetTextColor(new Color(222, 81, 81));
            //    noteInfo.Gravity = GravityFlags.Center;

            //    note.SetTextSize(ComplexUnitType.Pt, 8);
            //    note.Text = $"{carImage.Note}";
            //    note.Gravity = GravityFlags.Center;

            //    var dateInfo = new TextView(this);
            //    dateInfo.SetTextSize(ComplexUnitType.Pt, 6);
            //    dateInfo.Text = "Zrobiono";
            //    dateInfo.SetTextColor(new Color(222, 81, 81));
            //    dateInfo.Gravity = GravityFlags.Center;

            //    var date = new TextView(this);
            //    date.SetTextSize(ComplexUnitType.Pt, 8);
            //    date.Text = carImage.CreatedDateTime;
            //    date.Gravity = GravityFlags.Center;

            //    var imageView = new ImageView(this);
            //    imageView.Id = carImage.Id;
            //    imageView.SetMinimumWidth(500);
            //    imageView.SetMinimumHeight(700);
            //    imageView.Click += delegate
            //    {
            //        var intent = new Intent(this, typeof(ImageActivity));
            //        intent.PutExtra("imgUrl", carImage.PhotoName);
            //        StartActivity(intent);
            //    };

            //    RegisterForContextMenu(imageView);

            //    int height = 500;
            //    int width = 700;
            //    var bitmapCar = carImage.PhotoName.LoadAndResizeBitmap(width, height);
            //    if (bitmapCar != null)
            //    {
            //        imageView.SetImageBitmap(bitmapCar);
            //    }

            //    GC.Collect();

            //    lajt.AddView(imageView);
            //    var lpNote = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
            //    lpNote.SetMargins(0, 10, 0, 0);
            //    lajt.AddView(noteInfo, lpNote);
            //    lajt.AddView(note);
            //    lajt.AddView(dateInfo, lpNote);
            //    lajt.AddView(date);
            //}
        }
    }
}

