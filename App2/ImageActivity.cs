using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace App2
{
    [Activity(Label = "ImageActivity")]
    public class ImageActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.full_foto);
            var imgContainer = FindViewById<ImageView>(Resource.Id.fullFoto);
            var z = Intent.GetStringExtra("imgUrl");
            imgContainer.SetImageURI(Android.Net.Uri.Parse(z));
            // Create your application here
        }
    }
}