Someone asked me to create a "Building System" in Unity. Allowing you to "place" objects on other objects, thus building things.

You can see it in action in this youtube video: https://www.youtube.com/watch?v=rhlXP61Qxfs

Currently it can:
- Any object with a collider can be defined as a building surface, allowing other objects to be placed on it.
- Placement can happen freestyle anywhere on the surface, or limited to pre-defined "sockets", on both the surface and object.
- Rules for what can be placed where(Only pillar can be placed on corner sockets)
- Rotate objects when placing

This implementation is a rather early one, which I ended up improving quite bit for a project I was part of(This was the test for me joining).
It took me two days to create this, and I hope maybe others can benefit from seeing it =)
