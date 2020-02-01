namespace dotnet_example.Configuration
{
  public class RavenDB
  {
    public string[] DatabaseUrls { get; set; }
    public string DatabaseName { get; set; }
    public string CertPath { get; set; }
  }
}