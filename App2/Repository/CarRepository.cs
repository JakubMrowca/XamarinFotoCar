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
using App2.Models;
using Realms;

namespace App2.Repository
{
    public class CarRepository
    {
        private readonly Realm _db;

        public CarRepository()
        {
            var config = RealmConfiguration.DefaultConfiguration;
            config.SchemaVersion = 5;
            _db = Realm.GetInstance();
        }

        public List<Car> GetAllCar()
        {
            return _db.All<Car>().ToList();
        }

        public Car GetCarById(int id)
        {
            return _db.All<Car>().ToList().FirstOrDefault(x => x.Id == id);
        }

        public Car GetCarByWin(string win)
        {
            return _db.All<Car>().ToList().FirstOrDefault(x => x.WIN == win);
        }

        public int AddCar(Car newCar)
        {
            var allCars = GetAllCar();
            var number = 0;
            if (allCars.Count > 0)
                number = allCars.Max(x => x.Id);
            newCar.Id = number + 1;

            _db.Write(() => { _db.Add(newCar);});
            return newCar.Id;
        }

        public List<CarImage> GetAllCarsImages()
        {
            return _db.All<CarImage>().ToList();
        }

        public List<CarImage> GetCarImages(int carId)
        {
            return _db.All<CarImage>().ToList().Where(x => x.CarId == carId).ToList();
        }

        public int AddCarImage(CarImage newCarImage)
        {
            var allCarsImages = GetAllCarsImages();
            var number = 0;
            if (allCarsImages.Count > 0)
                number = allCarsImages.Max(x => x.Id);
            newCarImage.Id = number + 1;

            _db.Write(() => { _db.Add(newCarImage); });
            return newCarImage.Id;
        }

        public CarImage GetCarImage(int carImageId)
        {
            return _db.All<CarImage>().ToList().FirstOrDefault(x => x.Id == carImageId);
        }

        public bool DeleteImageFromCar(CarImage carImage)
        {
            var carId = carImage.CarId;
            _db.Write(()=>{_db.Remove(carImage);});
            var carImageCount = GetCarImages(carId).Count;
            if (carImageCount == 0)
            {
                var car = GetCarById(carId);
                _db.Write(() => { _db.Remove(car); });
                return true;
            }
            return false;
        }

        public void EditCarImage(int carImageId,int carId, string note)
        {
            var carImagetoEdit = GetCarImage(carImageId);
            _db.Write(() =>
            {
                carImagetoEdit.Note = note;
                carImagetoEdit.CarId = carId;
            });
        }
    }
}