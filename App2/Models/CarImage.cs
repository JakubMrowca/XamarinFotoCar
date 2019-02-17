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
using Realms;

namespace App2.Models
{
    public class CarImage:RealmObject
    {
        [Indexed]
        public int Id { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; }
        public string PhotoName { get; set; }
        public string Note { get; set; }
        public string CreatedDateTime { get; set; }

        public CarImage( )
        {
            CreatedDateTime = DateTime.Now.ToString();
        }
    }
}