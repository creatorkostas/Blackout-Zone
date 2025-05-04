Thank you for downloading Palette Modifier Lite. I hope this editor extension will help you make your game world more colourful and unique.
If you have any issues or feature requests, please contact me using the one of these links:

Forum Thread: http://bit.ly/palettemodifier
Discord Channel: https://discordapp.com/invite/RCdETwg
Email: johnq002@gmail.com

A video tutorial can be found at this link.
https://youtu.be/fLf4WSjlBPI

Rules and Tips:
- A texture atlas should be referenced by only a single Texture Grid scriptable object.
- The changes you make to the texture are not automatically saved to HDD. This means that if you make some changes to the texture atlas,
   and do not save them, those changes will be lost when you close the Unity Project.
- If you have several texture atlases that are just color variations of one another, you don’t need to create a Texture Grid 
   (create Flat Color Grids and Texture Rects) scriptable object for all of them. You can create a Texture Grid for one texture atlas, 
   duplicate it and replace the texture atlas it references.
- After you no longer need Palette Modifier, you can restore the original texture atlas format.
- If you add the Palette Modifier script to an object and notice that not all the object colors are in the inspector, most likely
   this is because in the Texture Grid not all flat colors are covered with a Flat Color Grid. Open the Grid Editor and make sure 
   all the flat colors and texture patterns are covered with a Flat Color Grid/Texture Pattern Rec. After that go to Misc tab and
   press Rebuild PM Data button.
      
