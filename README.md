# lucid-store
Distributed Key-Value Store Powered by Lucid Paxos
# About
Lucid Store is an ASP. NET Core Web API application implemented to serve as reliable, distributed key-value store.
# Formal Specification
The TLA+ specification of the Lucid Paxos algorithm is presented in the Lucid.tla file.
# Running the Store
Each "LucidReplicatedStore#" project is ready to build and run. Each project has an "appsettings.json" file that contains the configurations for the application (such as IP addresses and port numbers). In order to run a cluster of five instances of Lucid Store, change the IP addresses to "localhost" in "appsettings.json" files in all five instances of the "LucidReplicatedStore#" projects. Then run each instance separately.
# Environment
Lucid Store is ready to use on Windows and Linux. The only requirement is installing the latest version of .NET Core SDK and .NET Core runtime.
# Testing
An instance of Lucid Store is accessible via its network address that is specified in the corresponding "appsettings.json" file. NetworkAddresses[x] is the address for node with Ids[x], and so on.
