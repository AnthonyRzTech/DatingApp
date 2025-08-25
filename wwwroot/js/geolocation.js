// Store the Blazor component reference
window.blazorLocationComponent = null;

// Register the component instance
window.registerLocationComponent = function(componentRef) {
    window.blazorLocationComponent = componentRef;
};

// Get current location
window.getCurrentLocation = function() {
    if (!navigator.geolocation) {
        alert('Geolocation is not supported by your browser.');
        return;
    }
    
    navigator.geolocation.getCurrentPosition(
        function(position) {
            if (window.blazorLocationComponent) {
                window.blazorLocationComponent.invokeMethodAsync('UpdateLocation', 
                    position.coords.latitude, 
                    position.coords.longitude);
            }
        },
        function(error) {
            console.error('Geolocation error:', error);
            let errorMessage;
            switch(error.code) {
                case error.PERMISSION_DENIED:
                    errorMessage = "Location permission denied. Please enable location access in your browser settings.";
                    break;
                case error.POSITION_UNAVAILABLE:
                    errorMessage = "Location information is unavailable.";
                    break;
                case error.TIMEOUT:
                    errorMessage = "The request to get location timed out.";
                    break;
                default:
                    errorMessage = "Unable to get your location. Please enter it manually.";
            }
            alert(errorMessage);
        },
        {
            enableHighAccuracy: true,
            timeout: 10000,
            maximumAge: 0
        }
    );
};