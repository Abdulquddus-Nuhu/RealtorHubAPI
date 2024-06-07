# RealtorHubAPI

RealtorHubAPI is a comprehensive backend system designed for managing real estate listings. It allows users to perform CRUD operations on property listings, upload images and videos, and facilitates marketplace interactions.

## Key Features

### User Roles
- **Admin**: Oversees everything, and perform CRUD operations on all listings.
- **Realtor**: Manages property listings and uploads media.
- **Users**: Browse listings and make purchases.

### CRUD Operations
- Users can create, read, update, and delete property listings.
- Admins have the additional capability to oversee all listings and perform CRUD operations.

### Media Upload
- After creating a land listing, users can upload images and videos.
- MinIO is used to store the videos.
- For video files larger than 30MB, an endpoint generates a presigned URL to upload the files directly to the MinIO server.
- Once uploaded, the backend processes the videos using a long-running task in an ASP.NET Core background service:
  - Downloads the video from MinIO.
  - Uploads the video to Cloudinary for compression and transformation to reduce its size.
  - Downloads the compressed video from Cloudinary.
  - Reuploads the smaller file back to MinIO, replacing the original large file.

### Marketplace
- Users can browse the marketplace to see the list of available and unavailable lands.
- They can contact the admin for more information and initiate the purchase process.

### Payment Integration
- Integration with Flutterwave for secure payments.
- Webhooks are used to handle payment notifications and update the system accordingly.
- Admins can generate payment links and send them to users for property purchases.

### Deployment and Monitoring
- **GitHub CI/CD** is used for automated deployment to a Linux server.
- **Docker Compose** is used to run Prometheus, Grafana, and OpenTelemetry for monitoring the API.
- The application is hosted using **Nginx**.

### Secure Media Viewing
- When the frontend tries to view an image or video, it retrieves the URL from the database.
- A separate endpoint generates a temporary link for secure viewing, which expires after a certain period.

## Technologies Used
- **ASP.NET Core** for the backend API.
- **MinIO** for storing video files.
- **Cloudinary** for video compression and transformation.
- **ASP.NET Core Background Service** for processing media files.
- **Flutterwave** for payment processing.
- **Backgroung Queue** for managing long-running tasks.
- **GitHub CI/CD** for deployment.
- **Docker Compose** for running monitoring tools.
- **Prometheus** and **Grafana** for monitoring.
- **OpenTelemetry** for observability.
- **Nginx** for hosting.

## How It Works
1. **Property Management**: Users can manage property listings through standard CRUD operations. Admins can oversee and perform CRUD operations on all listings.
2. **Media Upload**:
   - Images and videos can be uploaded for each property.
   - Large videos (over 30MB) are handled using presigned URLs.
   - The backend processes these videos in the background, ensuring they are compressed and stored efficiently.
3. **Marketplace**: The platform displays all property listings, indicating their availability status.
4. **Payments**: Users can pay for properties directly through the integrated Flutterwave payment gateway. Admins can generate and send payment links to users.
5. **Monitoring**: The API is monitored using Prometheus, Grafana, and OpenTelemetry.
6. **Secure Media Viewing**: The frontend retrieves image and video URLs from the database and uses a secure endpoint to generate temporary links for viewing.


### Authentication and Authorization
- **JWT Tokens**: The application uses JWT tokens for authentication and authorization.
- **Role-Based Access**: Pages and actions are restricted based on user roles (Admin, Realtor, User).

### API Endpoint
The API is deployed and accessible at: [RealtorHubAPI Swagger Documentation](http://162.0.222.79:4080/swagger/index.html)

