using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace App3
{
    [Activity(Label = "ImageActivity")]
    public class ImageActivity : Activity
    {
        private Bitmap _bitmap;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.full_foto);
            var imgContainer = FindViewById<ImageView>(Resource.Id.fullFoto);
            var z = Intent.GetStringExtra("imgUrl");
            imgContainer.SetImageURI(Android.Net.Uri.Parse(z));


            //int height = Resources.DisplayMetrics.HeightPixels;
            //int width = imgContainer.Height;
            //_bitmap = z.LoadAndResizeBitmap(width, height);
            //if (_bitmap != null)
            //{
            //    imgContainer.SetImageBitmap(_bitmap);
            //    _bitmap = null;
            //}
            //// Dispose of the Java side bitmap.
            //GC.Collect();
            //imgContainer.SetImageURI(Android.Net.Uri.Parse(z));
            // Create your application here
        }
    }

    public static class Extension
    {
        public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
        {
            // First we get the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(fileName, options);

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