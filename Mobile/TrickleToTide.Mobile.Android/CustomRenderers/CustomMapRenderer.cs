using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TrickleToTide.Mobile.UI;
using TrickleToTide.Mobile.Droid.CustomRenderers;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;

[assembly: ExportRenderer(typeof(CustomMap), typeof(CustomMapRenderer))]
namespace TrickleToTide.Mobile.Droid.CustomRenderers
{
    public class CustomMapRenderer : MapRenderer, GoogleMap.IInfoWindowAdapter
    {
        public CustomMapRenderer(Context context) : base(context)
        {
        }

        public Android.Views.View GetInfoContents(Marker marker)
        {
            return null;
        }

        public Android.Views.View GetInfoWindow(Marker marker)
        {
            return null;
        }


        protected override void OnMapReady(GoogleMap map)
        {
            base.OnMapReady(map);

            NativeMap.SetInfoWindowAdapter(this);
        }


        protected override MarkerOptions CreateMarker(Pin pin)
        {
            var marker = new MarkerOptions();
            marker.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
            marker.SetTitle(pin.Label);
            marker.SetSnippet(pin.Address);

            // We store category in address
            switch (pin.Address.ToLowerInvariant())
            {
                case "paddle team":
                    marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pin_paddle_team));
                    break;

                case "walkers":
                    marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pin_walkers));
                    break;

                case "support":
                    marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pin_support));
                    break;

                default:
                    marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pin_default));
                    break;
            }
            return marker;
        }
    }
}