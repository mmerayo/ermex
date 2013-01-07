// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("ermeX.ConfigurationManagement.Settings")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("d5f11de7-410b-4193-ac37-c026ae2db8a7")]


//TODO: OPTIMIZE

[assembly:
    InternalsVisibleTo(
        "ermeX, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]
[assembly:
    InternalsVisibleTo(
        "ermeX.ConfigurationManagement, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]
[assembly:
    InternalsVisibleTo(
        "ermeX.ConfigurationManagement.Settings, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]
[assembly:
    InternalsVisibleTo(
        "ermeX.Common, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]
[assembly:
    InternalsVisibleTo(
        "ermeX.DAL, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]
[assembly:
    InternalsVisibleTo(
        "ermeX.Diagnostics, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]
[assembly:
    InternalsVisibleTo(
        "ermeX.Entities, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]
[assembly:
    InternalsVisibleTo(
        "ermeX.Temporal, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]
[assembly:
    InternalsVisibleTo(
        "ermeX.Temp.InterfacesBase, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]
[assembly:
    InternalsVisibleTo(
        "ermeX.Temp.InterfacesBase2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]
[assembly:
    InternalsVisibleTo(
        "ermeX.Transport, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]
[assembly:
    InternalsVisibleTo(
        "ermeX.Transport.BuiltIn.SuperSocket, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]
[assembly:
    InternalsVisibleTo(
        "ermeX.Transport.Interfaces, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]
[assembly:
    InternalsVisibleTo(
        "ermeX.Transport.PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]
[assembly:
    InternalsVisibleTo(
        "ermeX.Transport.Publish, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]
[assembly:
    InternalsVisibleTo(
        "ermeX.Transport.Reception, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]
[assembly:
    InternalsVisibleTo(
        "ermeX.Versioning, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
        )]