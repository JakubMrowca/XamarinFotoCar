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

namespace DataBase.Models
{
    public class CarImage:RealmObject
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; }
        public string PhotoName { get; set; }
        public string Note { get; set; }
        public DateTime CreatedDateTime { get; set; }

        public CarImage( )
        {
            CreatedDateTime = DateTime.Now;
        }
    }
}