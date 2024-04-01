# This file enables modules to be automatically managed by the Functions service.
# See https://aka.ms/functionsmanageddependency for additional information.
#
@{
    # For latest supported version, go to 'https://www.powershellgallery.com/packages/Az'. 
    # To use the Az module in your function app, please uncomment the line below.
    # 'Az' = '11.*'
    #'Az' = '9.*' is too slow, using specific modules instead: https://docs.microsoft.com/en-us/azure/azure-functions/functions-reference-powershell?tabs=portal#dependency-management
    #Version from: https://www.powershellgallery.com/packages?q=az
    'Az.Accounts' = '2.*'
    'Az.ContainerInstance' = '3.*'
}