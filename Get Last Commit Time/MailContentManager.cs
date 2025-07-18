using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Get_Last_Commit_Time
{
    internal class MailContentManager
    {
        public static string GenerateEventHtml(EventModel info)
        {
            var date = DateTime.Parse(info.eventDate);
            string month = date.ToString("MMM").ToUpper();
            string day = date.Day.ToString();

            return $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; border: 1px solid #ddd; border-radius: 8px; padding: 20px; margin: auto;'>

    <div style='display: flex; align-items: center; margin-bottom: 20px;'>
        <img src='{info.communityLogo}' alt='Community Logo' style='width: 70px; height: 70px; border-radius: 8px; margin-right: 15px; object-fit: cover;'>
        <div style='font-size: 20px; font-weight: bold; color: #333; line-height: 1.2;'>
            {info.communityName}<br/>{info.location}
        </div>
    </div>

    <div style='font-size: 22px; font-weight: bold; color: #222; margin-bottom: 15px;'>
        {info.eventName}
    </div>

    <div style='display: flex; align-items: center; margin-bottom: 10px;'>
        <div style='text-align: center; border: 1px solid #ccc; border-radius: 8px; width: 60px; font-weight: bold; font-size: 14px; color: #555; overflow: hidden;'>
            <div style='background-color: #f2f2f2; font-size: 10px; padding: 2px; color: #888;'>{month}</div>
            <div style='font-size: 20px; color: #000; padding: 5px 0;'>{day}</div>
        </div>
        <div style='margin-left: 10px;'>
            <div style='font-size: 16px; font-weight: bold; color: #333;'>{date:dddd, dd MMMM yyyy}</div>
            <div style='font-size: 14px; color: #666;'>Starts at {info.eventTime}</div>
        </div>
    </div>

    <div style='display: flex; align-items: center; margin-bottom: 10px;'>
        <div style='width: 40px; height: 40px; display: flex; justify-content: center; align-items: center; border: 1px solid #ccc; border-radius: 8px; font-size: 18px; color: #666; flex-shrink: 0; line-height: 1;'>
            📌
        </div>
        <div style='margin-left: 10px; flex: 1;'>
            <div style='font-weight: bold; color: #333;'>{info.eventVenue}</div>
        </div>
    </div>

    <div style='margin-top: 15px; font-size: 14px; color: #555; line-height: 1.5;'>
        {info.eventDescription}
    </div>

    <div style='margin-top: 20px; text-align: center;'>
        <a href='{info.eventLink}' style='background-color: #007BFF; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>
            More details on TamilNadu.Tech
        </a>
    </div>

    <div style='margin-top: 15px; font-size: 12px; color: #999; text-align: center;'>
        This is a community event notification via TamilNadu.Tech — we share events happening around Tamil Nadu.
    </div>

</div>";
        }


    }
}
