module raytracer
open System
open System.IO

type Vector3=
    struct
        val X : float
        val Y : float
        val Z : float

        member this.Magnitude() = ((this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z)) |> sqrt

        member this.Normalize() =
            let magnitude = this.Magnitude()
            Vector3(this.X / magnitude, this.Y / magnitude, this.Z / magnitude)

        member this.Dot(v: Vector3) =
            (this.X * v.X) + (this.Y * v.Y) + (this.Z * v.Z)
           
        static member (*) (v1:Vector3, v2: Vector3)=
            Vector3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z)

        static member (+) (v1:Vector3, v2: Vector3)=
            Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z)

        static member (-) (v1:Vector3, v2: Vector3)=
            Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z)

        static member (*) (a: float, v: Vector3)=
            Vector3(v.X * a, v.Y * a, v.Z *a)

        static member (*) ( v: Vector3, a: float)=
            Vector3(v.X * a, v.Y * a, v.Z *a)

        new(x, y, z) = {X = x; Y = y; Z = z}
        
        new (x) = {X = x; Y = x; Z = x}
        
    end

type Sphere= 
   struct  
      val Center: Vector3
      val Radius: float
      val Transparency: float
      val Reflectivity: float
      val Color : Vector3
      val EmissionColor: Vector3

      member this.Intersect(rayOrig: Vector3, rayDir: Vector3)=
             let diff = this.Center - rayOrig
             let doc = diff.Dot(rayDir) //distance origin to center
             if doc < 0.0 then None else
             let dcr = diff.Dot(diff) - (doc * doc) //distance from center of the sphere to the ray
             if dcr > (this.Radius * this.Radius) then None else
             let dic = ((this.Radius * this.Radius) - dcr) |> sqrt
             let t1 = doc - dic
             let t2 = doc + dic
             
             match (t1, t2) with
             | (t1, t2) when t1 < 0.0 -> Some (this, t2)
             | _ -> Some (this, t1)

      new(c, r, color, reflect, trans, ec) = {Center = c; Radius = r; Color = color; Transparency = trans; Reflectivity = reflect; EmissionColor = ec}
   end

let MAX_DEPTH = 5
let MOE = 1e-4
let SHADOW = 0.75
let mix a b m = b * m + a * (1.0 - m)

let minf x y = if x > y then y else x
let maxf x y = if x > y then x else y


let applyTile (s: Sphere) (i: Vector3) =
    if s.Center.Y > -1000.0 || (int((floor(i.X) + floor(i.Z))) % 2 <> 0) then s.Color else
        Vector3(0.0)
        
let correctNormal (rayDir: Vector3) (nhit: Vector3) =  if rayDir.Dot(nhit) > 0.0 then ((-1.0 * nhit), true) else (nhit, false)


let minSphere a b =
    let (_, t1) = a
    let (_, t2) = b
    if t1 < t2 then a else b

let rec trace rayOrig rayDir (spheres: Sphere list) depth=
    
    let hit = spheres |> List.map(fun s -> s.Intersect(rayOrig, rayDir)) |> List.reduce(fun x y -> x |> Option.fold (fun a b -> Some(a |> Option.fold minSphere b)) y)
 
    if hit.IsNone then Vector3(0.0, 0.0, 0.0) else
        
    let (s, i) = hit.Value
    let phit = rayOrig + rayDir * i
    let nhit, i = correctNormal rayDir ((phit - s.Center).Normalize())

    let mutable surfaceColor = (applyTile s phit)
  
    if (depth < MAX_DEPTH) then
        //let fr = (-1.0 * rayDir.Dot(nhit))

        //let fe = mix ((1.0-fr) * (1.0-fr) * (1.0-fr)) 1.0 0.1
        
        let rdir = (rayDir - (nhit * 2.0 * rayDir.Dot(nhit))).Normalize()
  
        let reflect = trace (phit + nhit * MOE) rdir spheres (depth + 1)
        

        surfaceColor <- (surfaceColor * Vector3(1.0-s.Reflectivity)) + (s.Reflectivity * reflect)
        
        let light = spheres.[spheres.Length - 1]
        
        let lightDir = (light.Center - phit).Normalize();
        let hit = spheres |> List.tryFind(fun x -> (x.Center <> light.Center) && x.Intersect(phit + MOE * nhit, lightDir).IsSome)
        if hit.IsSome then surfaceColor <- surfaceColor * (1.0 - SHADOW) else 
        surfaceColor <-  (surfaceColor * ((1.0-SHADOW) + SHADOW * (maxf (0.0) (nhit.Dot(lightDir)))))
        
    else ()

    surfaceColor + s.EmissionColor

let render (spheres: Sphere list)=
    let width = 1920.0
    let height = 1080.0
    let ar = width / height
    let fov = Math.PI / 2.0
    let angle = tan(fov / 2.0)
    let writer = new StreamWriter("lmao.ppm");
    writer.WriteLine("P6");
    writer.WriteLine(sprintf "%d %d" (int(width)) (int(height)));
    writer.WriteLine("255")
    writer.Flush();
    let arr = Array.create (int(width * height) * 3) (byte(0))
    for y in 0.0..(height - 1.0) do
        for x in 0.0..(width - 1.0) do
            let sx = (2.0 * ((x + 0.5) / width) - 1.0) * angle * ar
            let sy = (1.0 - 2.0 * ((y + 0.5) / height)) * angle
            //printf "[%f ,%f]\n" sx sy
            let rayDir = Vector3(sx, sy, -1.0).Normalize()
            let orig = Vector3(0.0, 0.0, 0.0)
           
            let c = trace orig rayDir spheres 0
            let i = int((y * width) + x) * 3
            arr.[i] <- byte(c.Y)
            arr.[i + 2] <- byte(c.X)
            arr.[i + 1] <- byte(c.Z)
    
    writer.BaseStream.Write(arr, 0, arr.Length)
    writer.Close();

  
    true
    

