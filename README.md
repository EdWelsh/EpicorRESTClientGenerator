# EpicorRESTClientGenerator
This genertor was created to maintain Business Objects in third party and custom integration software applications
This generator uses NSwag.CodeGeneration.CSharp 

I have included a WPF client for working with the class library

You may need to make a few modifications to the source code for your implementation 

I have included a ClientBase class that creates the HttpClient used to make all the rest calls, it also sets up the authentication
You may have to edit this class depending on your security needs
