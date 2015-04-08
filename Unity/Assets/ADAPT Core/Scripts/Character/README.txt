This folder contains the basic, generic forms for the components comprising the ADAPT character stack.

Coordinator (Lowest): The component responsible for fading choreographers (shadow controllers) in and out, and sending messages to them.

Body: An interface for the coordinator that translates messages and changes in blend weights to accessible animation commands. A character's basic motor skills are formally defined here.

Character: Sits above the Body and converts the Body's functions to blocking functions suitable for use in behavior tree nodes. A character's "capabilities" are formally defined here.

Behavior (Highest): The component that defines the character's behavior tree and handles high-level behavioral functions.
