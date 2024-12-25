<h1>ScaleSpoof</h1>

![ScaleSpoof Demo](https://github.com/user-attachments/assets/638f3a3b-6ff2-4720-b349-f67b6109ea42)

<p>This is a .NET program that allows you to spoof your display scale/DPI on a per-application basis via .dll injection.</p>
<p>In my experience this is most useful if you have a second monitor with a high pixel density that you have set to a higher scale (e.g. 150%), but you would still like your applications to render at 100% scale.</p>
<p>Essentially, imagine this, but per-application instead of per-monitor:</p>

![Windows Settings: Display scale](https://github.com/user-attachments/assets/1b6085b8-6cef-4e54-a1ca-2c6b894f40de)

<h2>Notes:</h2>
<ul>
  <li>x86 applications are currently unsupported.</li>
  <li>You must run the program as an administrator if you would like to spoof your DPI for privileged programs.</li>
</ul>

<h2>Caveats:</h2>
<ul>
  <li>Since it uses dll injection, this program can trigger anti-virus software. This is a false positive.</li)
  <li>x86 applications are currently unsupported. (They will crash if you try to inject them!)</li>
  <li>Elements in the non-client area (rendered by the shell), such as menu bars and title bars, currently do not scale.</li>
  <li>There are no guarantees that client-area elements will scale correctly either, especially GDI bitmaps/text or child windows.</li>
  <li>This program has not been thoroughly tested and there are no guarantees that it won't crash your application. (.dll injection is kind of inherently unsafe!)</li>
  <li>This only works on <b>DPI-aware</b> applications! It does not work on system-scaled applications.</li>
  <li>The program tries its best to tell your application to refresh its window with the new DPI, but in my experience it only works for some programs. You may need to drag your application's window between monitors to see any effects.</li>
  <li>There is currently no way to unload the injected .dll from an application without restarting it. (However, you can disable the effects of the program using the 'Disable' button!)</li>
</ul>

<p>As it stands I see this as more of a toy than a production-ready utility. <b>Don't blame me if you break something or lose unsaved work!</b></p>

<h2>Feature wishlist</h2>
<ul>
  <li>Automatic application hooking (you give the program the filename of an application, it sits in your system tray and hooks that application on startup)</li>
  <li>x86 support</li>
  <li>System-rendered text scaling?</li>
  <li>Proper window re-painting</li>
</ul>
