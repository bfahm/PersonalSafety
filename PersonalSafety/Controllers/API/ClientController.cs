using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Business;
using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels;
using PersonalSafety.Models.ViewModels.ClientVM;

namespace PersonalSafety.Controllers.API
{
    [ApiController]
    [Authorize]
    public class ClientController : ControllerBase
    {
        private readonly IClientBusiness _clientBusiness;
        private readonly ISOSBusiness _sosBusiness;
        private readonly ICategoryBusiness _categoryBusiness;
        private readonly IEventsBusiness _eventsBusiness;

        public ClientController(IClientBusiness clientBusiness, ISOSBusiness sosBusiness, ICategoryBusiness categoryBusiness, IEventsBusiness eventsBusiness)
        {
            _clientBusiness = clientBusiness;
            _sosBusiness = sosBusiness;
            _categoryBusiness = categoryBusiness;
            _eventsBusiness = eventsBusiness;
        }


        #region Registraion and Accounting

        /// <summary>
        /// Create a new client account to be able to access his services.
        /// </summary>
        /// <remarks>
        /// ## Main Functionality
        /// All the JSON object values **are** required and must follow these rules:
        /// 
        /// - **Email** : must be unique and not used before, additionally it must follow the correct email structure
        /// - **Password** : must be complex, contain number, symbols, Capital and Small letters
        /// - **NationalId** : must be exactly of 14 digits
        /// - **PhoneNumber** : must be exactly of 11 digits
        /// - **FullName** : Must be non null
        /// 
        /// After a valid attempt, this function also **automatically** sends a verification email to be used in `/api/Account/ConfirmMail` directly.
        /// **IMPORTANT:** User does not have access to any of the system's functionality till he actually verify his email.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-1]**: Invalid Request
        /// - User exsists and has registered before
        /// - Someone with the same National ID has registered before
        /// - Someone with the same Phone Number has registered before
        /// 
        /// #### **[-2]**: Identity Error
        /// This is a generic error code resembles something went wrong inside the Identity Framework and can be diagnosed using the response Messages list.
        /// </remarks>
        [AllowAnonymous]
        [HttpPost(ApiRoutes.Client.Registration)]
        public async Task<IActionResult> Register([FromBody] RegistrationViewModel request)
        {
            var authResponse = await _clientBusiness.RegisterAsync(request);

            if (authResponse.HasErrors)
            {
                return BadRequest(authResponse);
            }

            return Ok(authResponse);
        }
        /// <summary>
        /// Instantly log in with user's exsisting Facebook Account
        /// </summary>
        /// <remarks>
        /// ## Flow
        /// 1. Client Side user taps "Login with Facebook" button.
        /// 2. Client Side app finishes the heavy lifting of retrieving `access_token` **in the background** and saves it somewhere.
        /// 3. Client Side app sends a request to this method, ataching the `access_token` in the URI query.
        /// 4. If user was registered before, he is granted a JWT token, exactly similar to the one retrieved via `/Account/Login`
        ///     - If user was not registered before: the returned JWT token would be `null`, 
        ///     - Client Side app then utilizes `/RegisterViaFacebook` instead, also in the background and without user interaction.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-6]**: FacebookAuthError
        /// - Accesss Token is invalid or has expired
        /// </remarks>
        [AllowAnonymous]
        [HttpPost(ApiRoutes.Client.Registration)]
        public async Task<IActionResult> LoginViaFacebook([FromQuery] string accessToken)
        {
            var authResponse = await _clientBusiness.LoginWithFacebookAsync(accessToken);

            if (authResponse.HasErrors)
            {
                return BadRequest(authResponse);
            }

            return Ok(authResponse);
        }

        /// <summary>
        /// Register user using his exsisting Facebook Account
        /// </summary>
        /// <remarks>
        /// ## Flow
        /// 
        /// *This method exsists because registration to our services require other sensitive information that doesn't simply exsist, like the user's National Id and his Phone Number.*
        /// 
        /// 1. Client Side app redirects user to a page similar to the ordinary registration form, except that, this page won't ask user about information other than his:
        ///     - `NationalId`
        ///     - `PhoneNumber`
        /// 2. Client Side user finishes the form and taps "submit"
        /// 3. Client Side app sends a request to this method, ataching the `access_token` in the body, along his other info.
        /// 4. Upon `Status=0` response, meaning user with was successfully registered, Client Side app sends a new request, again, in the background, to `/LoginViaFacebook` which should return a valid JWT token, exactly similar to the one retrieved from `/Account/Login`.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-6]**: FacebookAuthError
        /// - Accesss Token is invalid or has expired
        /// #### **[-1]**: Invalid Request
        /// - User exsists and has registered before
        /// - Someone with the same National ID has registered before
        /// - Someone with the same Phone Number has registered before
        /// </remarks>
        [AllowAnonymous]
        [HttpPost(ApiRoutes.Client.Registration)]
        public async Task<IActionResult> RegisterViaFacebook([FromBody] RegistrationWithFacebookViewModel request)
        {
            var authResponse = await _clientBusiness.RegisterWithFacebookAsync(request);

            if (authResponse.HasErrors)
            {
                return BadRequest(authResponse);
            }

            return Ok(authResponse);
        }

        /// <summary>
        /// This method returns the profile of a logged in user
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// ## Main Functionality
        /// This method should be called before `Api/User/EditProfile` or any other calls that shows the list to the user before being able to modify them.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// *This method doesn't return any erros unless user is **UNAUTHORIZED***
        /// </remarks>
        [HttpGet(ApiRoutes.Client.Registration)]
        public async Task<IActionResult> GetProfile()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _clientBusiness.GetProfileAsync(currentlyLoggedInUserId);

            return Ok(response);
        }

        /// <summary>
        /// This method updates the basic information needed for the currently logged in user.
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// ## Main Functionality
        /// End-user can update his:
        /// - Full Name
        /// - Phone Number (Must be in the correct format)
        /// - National Id (Must be in the correct format)
        /// - Current Address
        /// - Blood Type
        ///     -  O = 1
        ///     -  A = 2
        ///     -  B = 3
        ///     -  AB = 4
        /// - Medical Notes (and history)
        /// - User Birthday in format similar to: `yyyy-mm-dd`
        /// 
        /// #### **IMPORTANT:** These fields are not mandatory, and not providing any values for them will result in the fields not updated. So users can only provide data to which they want to update.
        /// 
        /// User could also modify his current Emergency Contacts **by providing new ones here**.
        /// ####**IMPORTANT:** New emergency contacts replace old ones, so there is no need for separate delete/update calls for the sake of simplicity. Just provide the new setup and it will replace the old one.
        /// 
        /// ## Example for Usage
        /// ### First Time Login 
        /// All data provided along two emergency contacts
        /// 
        /// ```
        /// {
        ///     "currentAddress": "1841  Butternut Lane",
        ///     "bloodType": 1,
        ///     "medicalHistoryNotes": "Type A Diabetes",
        ///     "birthday": "1997-05-13",
        ///     "emergencyContacts": [
        ///         {
        ///             "name": "My Dad",
        ///             "phoneNumber": "012345678910"
        ///         },
        ///         {
        ///             "name": "My Mom",
        ///             "phoneNumber": "012345678910"
        ///         }
        ///     ]
        /// }
        /// ```
        /// 
        /// ### First Time Login 
        /// Only some of the data provided (Without emergency contacts and empty medical notes)
        /// 
        /// ```
        /// {
        ///     "currentAddress": "1841  Butternut Lane",
        ///     "bloodType": 1,
        ///     "birthday": "1997-05-13"
        /// }
        /// ```
        /// 
        /// ### Edit Profile
        /// Editing any value while logged in, after passing the first log in screen
        /// 
        /// ```
        /// {
        ///     "fullName": "John Doe",
        ///     "phoneNumber": "01234567890",
        ///     "nationalId": "01234567891011",
        ///     "currentAddress": "1841  Butternut Lane",
        ///     "bloodType": 1,
        ///     "medicalHistoryNotes": "Type A Diabetes",
        ///     "birthday": "1997-05-13",
        ///     "emergencyContacts": [
        ///         {
        ///             "name": "My Dad",
        ///             "phoneNumber": "012345678910"
        ///         },
        ///         {
        ///             "name": "My Mom",
        ///             "phoneNumber": "012345678910"
        ///         }
        ///     ]
        /// }
        /// ```
        /// 
        /// ### Deleting Emergency Contacts
        /// This call with empty data will cause the emergency contacts to be removed.
        /// 
        ///
        /// Other fields will remain to their state without changing though.
        /// ```
        /// {
        /// 
        /// }
        /// ```
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-2]**: IdentityError
        /// This is a generic error code resembles something went wrong inside the Identity Framework and can be diagnosed using the response Messages list.
        /// #### **[-4]**: NotConfrimed
        /// Could happen if the email matching the provided token was not verified.
        /// #### **[400]**: Bad Request
        /// Could happen if the provided PhoneNumber / NationalId is not in the correct format
        /// #### **[401]**: Unauthorized
        /// Could happen if the provided token in the header has expired or is not valid.
        /// </remarks>
        [HttpPut(ApiRoutes.Client.Registration)]
        public IActionResult EditProfile([FromBody] ProfileViewModel request)
        {
            //? means : If value is not null, retrieve it
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = _clientBusiness.EditProfile(currentlyLoggedInUserId, request);

            return Ok(response);
        }

        #endregion

        #region SOSRequests

        /// <summary>
        /// This method sends an SOS request for the current user.
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// ## Main Functionality
        /// User should be logged in so critical information about him could be assigned to the sent request. This action sends additional dynamic info about the user:
        /// - Authority Type:
        ///     - Police : 1
        ///     - Ambulance : 2
        ///     - Firefighting : 3
        ///     - TowTruck : 4
        /// - Request Location (Longitude and Latitude)
        /// - **(IMPORTANT)** ConnectionId: is an important string for tracking down the request evolution, new requests won't be sent unless a valid connection is established first with the server's SignalR endpoint.
        /// 
        /// #### **IMPORTANT:** These fields are not mandatory, and not providing any of the values will result in the request not being completed
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-1]**: InvalidRequest
        /// User failed to provide complete information.
        /// #### **[401]**: Unauthorized
        /// Could happen if the provided token in the header has expired or is not valid.
        /// </remarks>
        [HttpPost(ApiRoutes.Client.SOS)]
        public async Task<IActionResult> SendSOSRequest([FromBody] SendSOSRequestViewModel request)
        {
            //? means : If value is not null, retrieve it
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _sosBusiness.SendSOSRequestAsync(currentlyLoggedInUserId, request);

            return Ok(response);
        }

        /// <summary>
        /// This method cancels an SOS request via its Id.
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// ## Main Functionality
        /// User must only provide the Id of the request he/she wants to mark it as canceled, additionally **if** his connection is still maintained, the connectionId will be removed from the tracker.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[404]**: Notfound
        /// Could happen if the provided Id does not match an existing request.
        /// </remarks>
        [HttpPut(ApiRoutes.Client.SOS)]
        public async Task<IActionResult> CancelSOSRequest([FromQuery] int requestId)
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            var response = await _sosBusiness.CancelSOSRequestAsync(requestId, currentlyLoggedInUserId);

            return Ok(response);
        }

        /// <summary>
        /// This method updates the rate of the assigned rescuer.
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// ## Remarks
        /// User must provide the id of the request that was just solved. Ids of requests in other states will not be processed.
        ///
        /// Maximum Bounds: 5
        /// Minimum Bounds: 1
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[400]**: BadRequest
        /// The provided SOSRequestId does not map to a [Solved] Request.
        /// #### **[401]**: Unauthorized
        /// User does not have access to the provided SOS request.
        /// #### **[404]**: Notfound
        /// Could happen if the provided Id does not match an existing request.
        /// #### **[500]**: ServerError
        /// Could happen if the Solved Request was not assigne to a rescuer. This should not happen in usual scenarios, and is marked as a system error.
        /// </remarks>
        [HttpPost(ApiRoutes.Client.SOS)]
        public IActionResult RateRescuer([FromQuery] int requestId, [FromQuery] int rate)
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            var response =  _sosBusiness.RateRescuerAsync(currentlyLoggedInUserId, requestId, rate);

            return Ok(response);
        }

        /// <summary>
        /// Cancel any request that's waiting an outcome.
        /// </summary>
        /// <remarks>
        /// ### Functionality
        /// Searches for any pending request and cancels it. This unlocks the ability send new requests for clients.
        /// </remarks>
        [HttpPut(ApiRoutes.Client.SOS)]
        public async Task<IActionResult> CancelPendingRequests()
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            var response = await _sosBusiness.CancelPendingRequestsAsync(currentlyLoggedInUserId);

            return Ok(response);
        }

        /// <summary>
        /// Gets all ordered requests for a logged in client
        /// </summary>
        [HttpGet(ApiRoutes.Client.SOS)]
        public IActionResult GetSOSRequestsHistory()
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            var response = _sosBusiness.GetSOSRequestsHistory(currentlyLoggedInUserId);

            return Ok(response);
        }

        #endregion

        #region Events

        /// <summary>
        /// Update user location for tracking him and assigning him to a city cluster.
        /// </summary>
        /// <remarks>
        /// ### Remarks:
        /// This function should be called periodically, with the Longitude and Latitude of the user. The city where the user resides is then updated so he could be subscribed to notifications within the city or residancy.
        /// </remarks>
        [HttpPost(ApiRoutes.Client.Events)]
        public IActionResult UpdateLastKnownLocation([FromBody] LocationViewModel request)
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = _eventsBusiness.UpdateLastKnownLocation(currentlyLoggedInUserId, request);

            return Ok(response);
        }

        /// <summary>
        /// Update user's application registration for Firebase Notification
        /// </summary>
        /// <remarks>
        /// ### Remarks:
        /// This function should be called regularly whenever the app is updated / user logs in / registration key changes / etc.
        /// </remarks>
        [HttpPost(ApiRoutes.Client.Events)]
        public IActionResult UpdateDeviceRegistraionKey([FromBody] DeviceRegistrationViewModel request)
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = _eventsBusiness.UpdateDeviceRegistraionKey(currentlyLoggedInUserId, request);

            return Ok(response);
        }

        /// <summary>
        /// Gets all Categories of Events along there thumbnails (if available)
        /// </summary>
        [HttpGet(ApiRoutes.Client.Events)]
        public IActionResult GetEventsCategories()
        {
            var response = _categoryBusiness.GetEventCategories();

            return Ok(response);
        }

        /// <summary>
        /// Create a new event entry
        /// </summary>
        /// <remarks>
        /// ### Remarks:
        /// 
        /// #### Structure of the request
        /// - The title of the Event cannot be null.
        /// - The location of the Event cannot be null.
        /// 
        /// #### Event Category
        /// - A valid EventCategoryId can be retrieve from /GetEventsCategories()
        /// - EventCategoryId can be null if the event does not belong to a category
        /// - Using "Your Stories" and "Nearby Events" as categories is invalid and will result in nullified eventCategoryId field
        /// - Using an invalid category will result in a failed request.
        /// 
        /// #### File Upload
        /// - This function uses a Form instead of Raw Json as the Body of the request to support image upload.
        /// - Supported file types are: JPG, JPEG, PNG
        /// 
        /// #### Notification System
        /// - If an event is posted in city X (determined using the longitude and the latitude of the request), all residents of the city will get a push notification containing Data Payload, namely, a Silent Notification.
        /// - The payload will hold an updated state of all events in the city including the new one that was just posted in the form of dictionary.
        /// - The format of one item in the payload would be:
        ///     - KEY: "[Id]" of the event
        ///     - VALUE: "[latitude]_[longitude]"
        /// </remarks>
        [HttpPost(ApiRoutes.Client.Events)]
        public async Task<IActionResult> PostEvent([FromForm] PostEventRequestViewModel request)
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _eventsBusiness.PostEventAsync(currentlyLoggedInUserId, request);

            return Ok(response);
        }

        /// <summary>
        /// Gets a list of NEARBY events that can be filtered by a category
        /// </summary>
        /// <remarks>
        /// ### Remarks:
        /// - Filter is not a required parameter, and leaving it with a zero / null value will result in retreiving all the Events.
        /// - A valid filter value can by retreived by /GetEventsCategories
        /// - An invalid filter value will result in disregarding the filter and getting all events instead.
        /// </remarks>
        [HttpGet(ApiRoutes.Client.Events)]
        public async Task<IActionResult> GetEventsDetailed([FromQuery] int filter)
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _eventsBusiness.GetEventsDetailedAsync(currentlyLoggedInUserId, filter);

            return Ok(response);
        }

        /// <summary>
        /// Gets a single event using its Id
        /// </summary>
        [HttpGet(ApiRoutes.Client.Events)]
        public async Task<IActionResult> GetEventById([FromQuery] int eventId)
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _eventsBusiness.GetEventByIdAsync(currentlyLoggedInUserId, eventId);

            return Ok(response);
        }

        /// <summary>
        /// Cancels an event by its id
        /// </summary>
        /// <remarks>
        /// ### Remarks:
        /// 
        /// #### Notification System
        /// - If the event was posted in city X (determined using the longitude and the latitude of the request), all residents of the city will get a push notification containing Data Payload, namely, a Silent Notification.
        /// - The payload will hold an updated state of all events in the city including the new one that was just posted in the form of dictionary.
        /// - The format of one item in the payload would be:
        ///     - KEY: "[Id]" of the event
        ///     - VALUE: "[latitude]_[longitude]"
        /// </remarks>
        [HttpPut(ApiRoutes.Client.Events)]
        public async Task<IActionResult> CancelEventById([FromQuery] int eventId)
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _eventsBusiness.CancelEventByIdAsync(currentlyLoggedInUserId, eventId);

            return Ok(response);
        }

        /// <summary>
        /// Marks an event as Solved
        /// </summary>
        /// <remarks>
        /// ### Remarks:
        /// 
        /// #### Notification System
        /// - If the event was posted in city X (determined using the longitude and the latitude of the request), all residents of the city will get a push notification containing Data Payload, namely, a Silent Notification.
        /// - The payload will hold an updated state of all events in the city including the new one that was just posted in the form of dictionary.
        /// - The format of one item in the payload would be:
        ///     - KEY: "[Id]" of the event
        ///     - VALUE: "[latitude]_[longitude]"
        /// </remarks>
        [HttpPut(ApiRoutes.Client.Events)]
        public async Task<IActionResult> SolveEventById([FromQuery] int eventId)
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _eventsBusiness.SolveEventByIdAsync(currentlyLoggedInUserId, eventId);

            return Ok(response);
        }

        #endregion
    }
}