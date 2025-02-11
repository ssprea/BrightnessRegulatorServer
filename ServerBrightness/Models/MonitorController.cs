namespace ServerBrightnnes.Models;

public interface IMonitorController
{
    public void SetBrightness(uint brightness);

    public void SetContrast(uint contrast);

    public int GetBrightness();
    public int GetContrast();
}