#I "packages/build/FAKE/tools"
#r "FakeLib.dll"

open Fake

let configuration = environVarOrDefault "Configuration" (if buildServer = LocalBuild then "Debug" else "Release")
let solution = "ZabbixAgentLib.sln"

let dotNetAdditionalArgs =
    match TeamCityHelper.TeamCityBuildParameters.tryGet "build.counter" with
    | Some buildNumber -> [ sprintf "/p:BuildNumber=%s" buildNumber ]
    | _ -> []

Target "build" <| fun _ ->
    DotNetCli.Build (fun p ->
        { p with
            WorkingDir = "src"
            Configuration = configuration
            Project = solution
            AdditionalArgs = dotNetAdditionalArgs
        })

Target "pack" <| fun _ ->
    DotNetCli.Pack (fun p ->
        { p with
            WorkingDir = "src"
            Configuration = configuration
            Project = solution
            AdditionalArgs = dotNetAdditionalArgs
        })

    !! ("src\\**\\*.nupkg") |> Seq.iter TeamCityHelper.PublishArtifact

Target "ci" DoNothing

"build" ==> "pack"
"pack" ==> "ci"

RunTargetOrDefault "build"
