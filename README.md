# Forced-Perspective

* Forced Perspective illusion Mechanic inspired by SuperLiminal

* In a nutshell,

    * Use ray to detect intersection points, then put object there. The key idea put object as far as possible while scaling it to make appearance same in screen.

    * Object's scale is proportional to the distance between camera and object

        ![](https://upload.wikimedia.org/wikipedia/commons/thumb/a/af/Perspective_transform_diagram.svg/495px-Perspective_transform_diagram.svg.png)

        ![](https://wikimedia.org/api/rest_v1/media/math/render/svg/be4a118f91c4fc059e5de658030e927c203d234b)

    * that is, object_scale = init_scale * (hit.distance / Vector3.Distance(init_position - camera_init_position))

Reference:

* Most of the codes are from [danielcmcg/Forced-Perspective-Illusion-Mechanic-for-Unity](https://github.com/danielcmcg/Forced-Perspective-Illusion-Mechanic-for-Unity)