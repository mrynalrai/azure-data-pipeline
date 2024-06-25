# AzureDataPipeline
This project is a proof-of-concept that uses Azure's serverless functions to generate a sample zip file, upload it to Azure Storage, and later process it to save the data in Azure SQL Database. This PoC demonstrates the automation of data collection, compression, upload, and processing using serverless technologies, which can be applied to various real-life scenarios.

# Components
### HttpFunctionApp
An HTTP-triggered Azure Function that generates a compressed zip file from a text file and uploads it to Azure Storage.

### EventGridFunctionApp
An EventGrid-triggered Azure Function that processes new files uploaded to Azure Storage and saves the data in Azure SQL Database.

### LaunchDockerFunctionApp
An HTTP-triggered PowerShell Azure Function that creates a new Azure Container Instance.

# Real-Life Scenario
## Automated Data Collection and Processing for a Research Organization
Scenario
A research organization collects large volumes of data from various sources (e.g., field data, survey results, sensor data) in text file format. These text files need to be compressed, securely uploaded to a central storage, processed, and the relevant data extracted and saved into a database for further analysis.

### Solution using the PoC:
**HttpFunctionApp**: Automated Data Collection and Compression
* Researchers or automated systems generate text files containing raw data.
* The HttpFunctionApp Azure Function is triggered, which compresses the text files into a zip file and uploads it to Azure Blob Storage.
* This ensures that data is securely and efficiently transferred to the cloud.

**EventGridFunctionApp**: Data Processing and Storage
* Once the zip file is uploaded, an EventGrid event is triggered.
* The EventGridFunctionApp Azure Function processes the new zip file. It extracts the contents, parses the text files, and processes the data (e.g., data cleansing, validation).
* The processed data is then saved into Azure SQL Database for further analysis by the research team.

**LaunchDockerFunctionApp**: On-Demand Computational Resources
* Occasionally, researchers need to run complex data analysis or simulations that require additional computational resources.
* The LaunchDockerFunctionApp can be triggered via HTTP to create a new Azure Container Instance. This instance can run custom Docker containers with the necessary tools and libraries for advanced data analysis.
* This allows the organization to scale its computational resources dynamically based on demand.

### Benefits
* **Automation**: Streamlines the entire process from data collection to storage, reducing manual effort and errors.
* **Scalability**: Utilizes Azure's serverless architecture to handle varying loads efficiently.
* **Security**: Ensures data is securely uploaded and processed in the cloud.
* **Cost-Effective**: Only uses resources as needed, minimizing costs compared to maintaining always-on infrastructure.
* **Flexibility**: Easily adaptable to different types of data and processing requirements.

# Azure Resources Used
* Azure Functions
* Azure SQL Database
* Azure Event Grid
* Azure Container Instances