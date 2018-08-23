using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Image360
{
    public class Image360 : ContentView
    {
        public static readonly BindableProperty SourceProperty = BindableProperty.Create("Source", typeof(string), typeof(Image360), propertyChanged: OnSourcePropertyChanged);
        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        public static readonly BindableProperty IsLoadingProperty = BindableProperty.Create("IsLoading", typeof(bool), typeof(Image360), false);
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        Image360Sphere sphere;
        UrhoSurface urhoSurface;
        public Image360()
        {
            urhoSurface = new UrhoSurface();
            urhoSurface.VerticalOptions = LayoutOptions.FillAndExpand;
            this.Content = urhoSurface;

        }
        private static void OnSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as Image360).LoadImage(newValue as string);
        }
        private async void LoadImage(string imurl)
        {
            IsLoading = true;
            try
            {
                if (sphere == null)
                    sphere = await urhoSurface.Show<Image360Sphere>(new ApplicationOptions(assetsFolder: null) { Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait });

                await sphere.SetImage(imurl);
            }
            finally
            {
                IsLoading = false;
            }
        }

    }
}
