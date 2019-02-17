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

namespace App3.Models
{
    public class Car:RealmObject
    {
        public string WIN { get; set; }
        [Indexed]
        public int Id { get; set; }
        public string Description { get; set; }
        public  IList<CarImage> Images { get; }
        public string CreatedDateTime { get; set; }

        public Car()
        {
            CreatedDateTime = DateTime.Now.ToString("dd/MM/yyyy hh:mm");
        }
    }
}