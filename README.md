# Microservices File Upload System

A secure, scalable file upload system built with .NET 8 using microservices architecture and pre-signed URLs.

## Architecture Overview

```
┌─────────────────┐    ┌─────────────────┐
│   App Service   │────│ Storage Service │
│                 │    │                 │
│ - Authentication│    │ - Pre-signed    │
│ - Product Mgmt  │    │   URL Generation│
│ - JWT Tokens    │    │ - File Storage  │
│ - API Gateway   │    │ - Signature     │
│                 │    │   Verification  │
└─────────────────┘    └─────────────────┘
         │                       │
         └───────────────────────┘
              Secure Communication
```

## Features

- **Secure Authentication**: JWT-based authentication system
- **Pre-signed URLs**: Secure file upload mechanism with expiration
- **Message Integrity**: HMAC-SHA256 signature verification
- **Microservice Architecture**: Clean separation of concerns
- **Docker Support**: Full containerization with Docker Compose
- **API Documentation**: Swagger/OpenAPI integration
- **Comprehensive Logging**: Structured logging throughout

## Services

### App Service (Port 5003)
- **Authentication**: `/api/auth/login`
- **Upload URL Generation**: `/api/upload/request-url`  
- **Product Management**: `/api/product`

### Storage Service (Port 5004)
- **Pre-signed URL Generation**: `/api/storage/presigned-url`
- **File Upload**: `/api/storage/upload/{token}`
- **Image Validation**: `/api/storage/validate-image`
- **Image Retrieval**: `/api/storage/image/{imageId}`

## Quick Start

### Prerequisites
- Docker and Docker Compose
- .NET 8 SDK (for local development)

### Running with Docker Compose

1. Clone the repository
2. Navigate to the project root
3. Run the services:

```bash
docker-compose up --build
```

### Service URLs
- **App Service**: http://localhost:5003
- **Storage Service**: http://localhost:5004
- **App Service Swagger**: http://localhost:5003/swagger/index.html
- **Storage Service Swagger**: http://localhost:5004/swagger/index.html

## Workflow

### 1. Authentication
```bash
POST http://localhost:5003/api/auth/login
Content-Type: application/json

{
  "username": "seller",
  "password": "password123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2024-01-20T12:00:00Z"
  }
}
```

### 2. Request Upload URL
```bash
POST http://localhost:5003/api/upload/request-url
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "fileName": "product-image.jpg",
  "contentType": "image/jpeg",
  "fileSize": 1024000,
  "productId": "product-123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Upload URL generated successfully",
  "data": {
    "uploadUrl": "http://localhost:5004/api/storage/upload/abc123...",
    "token": "abc123...",
    "expiresAt": "2024-01-20T11:15:00Z"
  }
}
```

### 3. Upload File
```bash
POST {upload-url-from-step-2}
Content-Type: multipart/form-data

[Binary file data]
```

**Response:**
```json
{
  "success": true,
  "message": "File uploaded successfully",
  "data": {
    "imageId": "def456...",
    "message": "File uploaded successfully"
  }
}
```

### 4. Create Product
```bash
POST http://localhost:5003/api/product
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "name": "Awesome Product",
  "description": "This is an awesome product",
  "price": 99.99,
  "imageId": "def456..."
}
```

**Response:**
```json
{
  "success": true,
  "message": "Product created successfully",
  "data": {
    "id": "ghi789...",
    "name": "Awesome Product",
    "description": "This is an awesome product",
    "price": 99.99,
    "imageId": "def456...",
    "createdAt": "2024-01-20T10:30:00Z"
  }
}
```

## Security Features

### 1. JWT Authentication
- Secure token-based authentication
- Configurable expiration times
- Bearer token authorization

### 2. Message Integrity
- HMAC-SHA256 signature verification
- Prevents tampering with upload metadata
- Signature includes all critical file information

### 3. Upload Validation
- Token expiration enforcement
- File metadata verification (name, type, size)
- Signature verification before upload

### 4. Secure Inter-Service Communication
- Shared secret key for signature generation/verification
- Request validation at service boundaries

## Development

### Local Development Setup

1. **Install .NET 8 SDK**
2. **Clone and navigate to project**
3. **Run App Service:**
   ```bash
   cd AppService
   dotnet run
   ```
4. **Run Storage Service:**
   ```bash
   cd StorageService
   dotnet run
   ```

### Project Structure
```
├── AppService/
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── Program.cs
│   ├── Dockerfile
│   └── AppService.csproj
├── StorageService/
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── Program.cs
│   ├── Dockerfile
│   └── StorageService.csproj
├── docker-compose.yml
└── README.md
```

## Configuration

### Environment Variables
- `JWT__SecretKey`: Secret key for JWT token generation
- `JWT__Issuer`: JWT token issuer
- `JWT__Audience`: JWT token audience
- `StorageService__BaseUrl`: Base URL for Storage Service

### File Storage
- Files are stored in the `./uploads` directory
- Directory is automatically created if it doesn't exist
- Images are accessible via `/api/storage/image/{imageId}`

## API Documentation

Both services include Swagger/OpenAPI documentation:
- **App Service**: http://localhost:5003/swagger
- **Storage Service**: http://localhost:5004/swagger

## Monitoring and Logging

- Structured logging using .NET's built-in logging
- Request/response logging for troubleshooting
- Error handling with appropriate HTTP status codes
- Comprehensive log messages for all operations

## Testing

### Manual Testing with curl

#### 1. Login
```bash
curl -X POST "http://localhost:5003/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"seller","password":"password123"}'
```

#### 2. Request Upload URL
```bash
curl -X POST "http://localhost:5003/api/upload/request-url" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{"fileName":"test.jpg","contentType":"image/jpeg","fileSize":1024,"productId":"test-product"}'
```

#### 3. Upload File
```bash
curl -X POST "UPLOAD_URL_FROM_STEP_2" \
  -F "file=@/path/to/your/image.jpg"
```

#### 4. Create Product
```bash
curl -X POST "http://localhost:5003/api/product" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Product","description":"Test Description","price":29.99,"imageId":"IMAGE_ID_FROM_UPLOAD"}'
```

## Troubleshooting

### Common Issues

1. **Authentication Failed**: Ensure you're using the correct credentials (`seller`/`password123`)
2. **Upload Token Expired**: Upload tokens expire in 15 minutes - request a new URL
3. **File Metadata Mismatch**: Ensure uploaded file matches exactly the metadata provided
4. **Service Communication**: Check that both services are running and can communicate

### Logs
Check Docker logs for detailed error information:
```bash
docker-compose logs app-service
docker-compose logs storage-service
```

## Future Enhancements

- **Message Queues**: Add RabbitMQ for async processing
- **Distributed Logging**: Integrate with ELK stack
- **File Compression**: Add image optimization
- **Cloud Storage**: Support for AWS S3, Azure Blob Storage
- **Rate Limiting**: API rate limiting and throttling
- **Health Checks**: Service health monitoring endpoints