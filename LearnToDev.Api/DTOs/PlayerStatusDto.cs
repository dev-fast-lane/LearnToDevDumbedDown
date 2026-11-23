namespace LearnToDev.Api.DTOs;

// Det startskjermen trenger å vite om spilleren
public class PlayerStatusDto
{
    //null betyr at spilleren ikke har jobb
    public string? CurrentJobName { get; set; }
    
    //Navnene på skolene spilleren har bestått. Tom liste hvis ingen
    public List<string> PassedSchools { get; set; } = new();
}