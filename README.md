# Physical activity tracker for Meta Quest
### By Sindri Petursson, sindripet@gmail.com

## Created for master's thesis at KTH in Interactive Media Technology in 2023
Making virtual reality game players more physically active and immersed in the gameplay by involving their physical activity data.

## Implementation
The physical activity bar is designed with the goal to increase players' physical activity, beneficial movements, and immersion while playing a VR game. The bar gets filled up as the VR player physically moves by gaining a small number of points for every movement made, but then by a larger amount by performing specific beneficial movements (squats, jumping jacks, and side jacks). The player’s movements are tracked from three tracking points, from both hand controllers and the VR headset.

The solution can be integrated into different VR games with minimal modifications required. The solution is a user interface with a physical activity bar where the player gets an in-game reward upon filling it, the reward would depend on the game utilizing it. It could for example be a health refill, extra points to increase the player’s score, or additional time to complete a challenging task. The difficulty level for filling up the physical activity bar and the corresponding reward can be determined by the game designer of the VR game utilizing it. The design is highly flexible and can be easily customized to suit specific gameplay and preferences.

## Beneficial movements
- Squats are detected when the players’ head gets under 30% of their height. The head has to be facing forward, not tilted more than 45 degrees down, to count as a legal squat. The player has to get his head back to its upstanding height to be triggered again.
- Jumping jacks are detected when the player simultaneously jumps and moves their hands above their head. Is also detected when the player has their hands above their head, performs a jump, and subsequently moves their hands below their head. The player can alternate between these two stages as wanted.
- Side jacks are detected when one of the player’s hands is above the head while keeping the other hand down, and simultaneously tilting their head in the opposite direction to the raised hand. For example, when the right hand is raised, the head should tilt to the left, and vice versa.

## Scripts
- ActivityMonitor.cs - Movement tracking and beneficial movement detection.
- UIManager.cs - UI with the physical activity bar.
