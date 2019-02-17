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
using App2.Models;

namespace App2.Components
{

    public class WinSelectDialog
    {
        private readonly Activity _activity;
        private readonly AlertDialog.Builder _builder;
        private readonly AutoCompleteTextView _autoCompleteText;
        private readonly TextView _noteText;
        private readonly LinearLayout _linearLayout;

        public string Win => _autoCompleteText.Text;
        public string Note => _noteText.Text;
        

        private Orientation _orientation = Orientation.Vertical;

        public WinSelectDialog(Activity activity)
        {
            _activity = activity;
            _builder = new AlertDialog.Builder(activity);
            _autoCompleteText = new AutoCompleteTextView(activity);
            _linearLayout = new LinearLayout(activity);
            _noteText = new TextView(activity);
        }

        private void AddAutocompleteToLayout(string [] autocomplete, int resource, string title)
        {
            _linearLayout.AddView(new TextView (_activity){Text = title});
            var adapter = new ArrayAdapter(_activity, resource, autocomplete);
            _autoCompleteText.Adapter = adapter;
            _linearLayout.AddView(_autoCompleteText);
        }

        private void SetButton(string title, EventHandler<DialogClickEventArgs> click,
            IDialogInterfaceOnClickListener cancel = null)
        {
            if (cancel != null)
            {
                _builder.SetPositiveButton(title, click);
                _builder.SetNegativeButton("Anuluj", click);
            }
            else
                _builder.SetPositiveButton(title, click);
        }

        private void AddTextToLayout(string title)
        {
            _linearLayout.AddView(new TextView(_activity){Text = title});
            _linearLayout.AddView(_noteText);
        }

        public void Build(string[] autocomplete, EventHandler<DialogClickEventArgs> click)
        {
            AddAutocompleteToLayout(autocomplete, Android.Resource.Layout.SimpleDropDownItem1Line, "WIN");
            AddTextToLayout("Notatka");
            SetButton("Dodaj", click);

            _linearLayout.Orientation = _orientation;
            _builder.SetTitle("Nowe zdjęcię");
            _builder.SetView(_linearLayout);
        }

        public void Show()
        {
            _builder.Show();
        }

        public void SetOrientation(Orientation orientation)
        {
            _orientation = orientation;
        }
    }
}