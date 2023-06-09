﻿# Image Steganography LSB (Least Significant Bit)
<img src="AboutProject/Image-Steganography-demo.gif" alt="Dashboard">

.Net application that utilizes an LSB algorithm to hide text in images through a Telegram bot.

## Description

Image steganography is a method of concealing confidential details within an image file so that it cannot be detected by humans. LSB substitution is the most common algorithm utilized in this practice. This involves modifying the least significant bit of specific pixels to represent a character or number.

The algorithm used in this program uses binary. Images are made up of pixels. Each pixel is a 24-bit string (I calculated each color separately, which gives us more memory) that represents the color of that pixel. For example, the string 00000000 represents white and 11111111 black colors. Now suppose we change a white pixel color from 00000000 to 00000001, i.e., we change its least significant bit, the user's eye cannot detect it. Even with lots of zooming, it is hard to detect it.

In the link section, you can learn more about it.

## Getting Started

`.NET 7` is used for the server side of this application, and `System.Drawing.Common` is being used to read pixels, which is available only for Windows. In order to use this program under Linux, change the reading bitmap sections with another library. Telegram bot was used for the user interface.

### Dependencies

- Visual Studio (Preferrable IDE)
- .Net 7
- Telegram Account

### Installing

Edit the **appsettings.json** file and replace the credentials with your bot:

```
  "TelgeramBotKey": "EnterYourKey",
```

## License

This project is licensed under the MIT License

## Links

- [Image Steganography](https://zbigatron.com/image-steganography-simple-examples/)
- [Telegram Bot](https://github.com/TelegramBots/Telegram.Bot)
