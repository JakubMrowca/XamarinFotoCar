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
using App3.Models;

namespace App3.Components
{

    public class WinSelectDialog
    {
        private readonly Activity _activity;
        private readonly AlertDialog.Builder _builder;
        private readonly AutoCompleteTextView _autoCompleteText;
        private readonly EditText _noteText;
        private readonly LinearLayout _linearLayout;

        public string Win => _autoCompleteText.Text;
        public string Note => _noteText.Text;
        

        private Orientation _orientation = Orientation.Vertical;
        private string _title = "Nowe zdjęcię";
        private string _buttonTitle = "Dodaj";
        private string _win = null;
        private string _note = null; 

        public WinSelectDialog(Activity activity,string title,string win = null,string selectedNote = null)
        {
            _title = title;
            _activity = activity;
            _builder = new AlertDialog.Builder(activity);
            _autoCompleteText = new AutoCompleteTextView(activity);
            _linearLayout = new LinearLayout(activity);
            _win = win;
            _note = selectedNote;
            _noteText = new EditText(activity);
        }

        private void AddAutocompleteToLayout(string [] autocomplete, int resource, string title)
        {
            var lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
            lp.SetMargins(50,5,50,5);
            _linearLayout.LayoutParameters = lp;
            _linearLayout.AddView(new TextView (_activity){Text = title},lp);
            var adapter = new ArrayAdapter(_activity, resource, autocomplete);
            
            _autoCompleteText.Adapter = adapter;
            if (_win != null)
            {
                _autoCompleteText.Text = _win;
                _autoCompleteText.SelectAll();
            }

            if (_note != null)
            {
                _noteText.Text = _note;
                _noteText.SelectAll();
            }
            _linearLayout.AddView(_autoCompleteText,lp);
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

        public void AddTextToLayout(string title)
        {
            var lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
            lp.SetMargins(50, 5, 50, 10);

            _linearLayout.AddView(new TextView(_activity){Text = title },lp);
            _linearLayout.AddView(_noteText, lp);
        }

        public void Build(string[] autocomplete, EventHandler<DialogClickEventArgs> click,bool withNote= true)
        {
            AddAutocompleteToLayout(autocomplete, Android.Resource.Layout.SimpleDropDownItem1Line, "WIN");
            SetButton(_buttonTitle, click);
            if(withNote)
                AddTextToLayout("Notatka");
            _linearLayout.Orientation = _orientation;
            _builder.SetTitle(_title);
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

        public void SetButtonTitle(string buttonTitle)
        {
            _buttonTitle = buttonTitle;
        }

        public void SetDissmisEvent(OnDismissListener listener)
        {
            _builder.SetOnDismissListener(listener);
        }

    }

    public sealed class OnDismissListener : Java.Lang.Object, IDialogInterfaceOnDismissListener
    {
        private readonly Action action;

        public OnDismissListener(Action action)
        {
            this.action = action;
        }

        public void OnDismiss(IDialogInterface dialog)
        {
            this.action();
        }
    }
}