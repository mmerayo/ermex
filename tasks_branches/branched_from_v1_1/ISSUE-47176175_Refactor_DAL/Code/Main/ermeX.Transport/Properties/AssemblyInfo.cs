// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
// /*---------------------------------------------------------------------------------------*/
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("ermeX.Transport")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("e88e7964-4261-4a9d-87ec-9dd44be8711e")]

//TODO: fix visibilities

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
        "ermeX.Models, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
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
        "ermeX.Transport.IoC, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290c7c9c274cbafdecf926139ffe02444d6d8e1d3522e51ba94d386d898b5693d007852d34b443ff530e30a20f39bf2193d77c174831ca849ac2da46dcbde47e728c88a58ca0e281f8aea995e5b2c301de286b55694926db998a571f0e1ec7eb956f5f3488e18e9aa15339d5fc4c6133083bf3cb3aa6f9a467edfe8bbbee0ec1"
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