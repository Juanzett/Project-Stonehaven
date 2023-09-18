using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    
    public enum Season  { NONE, Summer, Autumn, Winter, Spring };
    public enum Weather { NONE, Rainy, Sunny, Snowy, Cloudy };

    public Season currentSeason;
    public Weather currentWeather;

    [Header ("Time Settings")]
    public float seasonTime;
    public float springTime;
    public float summerTime;
    public float autumnTime;
    public float winterTime;

    public int currentYear;

    void Start() 
    {
      this.currentSeason = Season.Spring;
      this.currentWeather = Weather.Sunny;
      this.currentYear = 1;

      this.seasonTime = this.springTime;
    }

    void Update() 
    {
        
    }

    public void ChangeSeason(Season seasonType)
    {
        if(seasonType != this.currentSeason)
        {
            switch (seasonType)
            {
                case Season.Spring:
                currentSeason = Season.Spring;
                break;
                case Season.Summer:
                currentSeason = Season.Summer;
                break;
                case Season.Autumn:
                currentSeason = Season.Autumn;
                break;
                case Season.Winter:
                currentSeason = Season.Winter;
                break;
            }
        }
    }

    public void ChangeWeather(Weather weatherType)
    {
        if(weatherType != this.currentWeather)
        {
            switch (weatherType)
            {
                case Weather.Rainy:
                currentWeather = Weather.Rainy;
                break;
                case  Weather.Snowy:
                currentWeather =  Weather.Snowy;
                break;
                case  Weather.Sunny:
                currentWeather =  Weather.Sunny;
                break;
                case  Weather.Cloudy:
                currentWeather =  Weather.Cloudy;
                break;
            }
        }
    }
}
