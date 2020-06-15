// Learn more about F# at http://fsharp.org

open System
open raytracer

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    let spheres = [Sphere(Vector3(0.0, -10004.0, -20.0), 10000.0, Vector3(205.0, 133.0, 63.0), 0.4, 0.0,  Vector3(0.0))
                   Sphere(Vector3(-5.0, 0.0, -14.0), 4.0, Vector3(0.0, 255.0, 0.0), 0.4, 0.0, Vector3(0.0))
                   Sphere(Vector3(0.0, 4.0, -24.0), 8.0, Vector3(255.0, 0.0, 0.0), 0.4, 0.0, Vector3(0.0))
                   Sphere(Vector3(5.0, 0.0, -14.0), 4.0, Vector3(0.0, 0.0, 255.0), 0.4, 0.0, Vector3(0.0))
                   Sphere(Vector3(2.0, 10.0, 1.0), 2.0, Vector3(0.00, 0.00, 0.00), 0.0, 0.0, Vector3(255.0))          
                  ]
    render spheres |> ignore
    printfn "done"
    0 // return an integer exit code
