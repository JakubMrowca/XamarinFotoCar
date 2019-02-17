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
    public class Car:RealmObject
    {
        public string WIN { get; set; }
        public int Id { get; set; }
        public string Description { get; set; }
        public  IList<CarImage> Images { get; set; }
        public DateTime CreatedDateTime { get; set; }

        public Car()
        {
            CreatedDateTime = DateTime.Now;
        }
    }
}