using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Data.UtilityNetwork.Telecom;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace CircuitManagementDemo
{
  internal class CircuitManagementPaneViewModel : DockPane
  {
    private const string _dockPaneID = "CircuitManagementDemo_CircuitManagementPane";
    private Map _map;
    private FeatureLayer _featureLayer;
    private string _utilityNetworkName = "Network";

    protected CircuitManagementPaneViewModel()
    {
      _map = MapView.Active.Map;
      _featureLayer = _map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault();
    }

    #region MVVM Helpers

    private string _circuitName = null;
    public string CircuitName
    {
      get => _circuitName;
      set
      {
        SetProperty(ref _circuitName, value, () => CircuitName);
      }
    }

    private ICommand _createCircuitCommand = null;
    public ICommand CreateCircuitCommand
    {
      get
      {
        if (_createCircuitCommand == null)
        {
          _createCircuitCommand = new RelayCommand(CreateCircuit);
        }
        return _createCircuitCommand;
      }
    }
    private ICommand _alterCircuitCommand = null;
    public ICommand AlterCircuitCommand
    {
      get
      {
        if (_alterCircuitCommand == null)
        {
          _alterCircuitCommand = new RelayCommand(AlterCircuit);
        }
        return _alterCircuitCommand;
      }
    }

    private ICommand _verifyCircuitCommand = null;
    public ICommand VerifyCircuitCommand
    {
      get
      {
        if (_verifyCircuitCommand == null)
        {
          _verifyCircuitCommand = new RelayCommand(VerifyCircuit);
        }
        return _verifyCircuitCommand;
      }
    }

    private ICommand _deleteCircuitCommand = null;
    public ICommand DeleteCircuitCommand
    {
      get
      {
        if (_deleteCircuitCommand == null)
        {
          _deleteCircuitCommand = new RelayCommand(DeleteCircuit);
        }
        return _deleteCircuitCommand;
      }
    }

    #endregion MVVM Helpers

    /// <summary>
    /// Creates a non-sectioned circuit between two telecom elements in the utility network.
    /// </summary>
    private void CreateCircuit()
    {
      QueuedTask.Run(() =>
      {
        // OLT FROM
        // 16 ports (FirstUnit 1, LastUnit 16) are available on this element
        string startGlobalID = "1F16BF90-4B7E-4E1E-82B2-5AEFCF86C82E";

        //OLT TO
        // 16 ports (FirstUnit 1, LastUnit 16) are available on this element
        string stopGlobalID = "19F271EE-E25B-45D0-B671-6AC4C02E3DB7";

        using (Geodatabase geodatabase = _featureLayer.GetFeatureClass().GetDatastore() as Geodatabase)
        using (UtilityNetwork utilityNetwork = geodatabase.OpenDataset<UtilityNetwork>(_utilityNetworkName))
        using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
        {

          if (!utilityNetwork.HasTelecomNetwork)
          {
            MessageBox.Show("The utility network does not have a telecom domain network");
          }


          // Get the telecom domain network from the utility network definition.
          TelecomDomainNetwork telecomDomainNetwork = utilityNetworkDefinition.GetDomainNetwork("Telco") as TelecomDomainNetwork;

          using (NetworkSource networksource = telecomDomainNetwork.NetworkSources.First(f => f.Name.Replace(" ", "").Contains("TelcoJunctionObject")))
          using (AssetGroup assetGroup = networksource.GetAssetGroup("Port"))
          using (AssetType assetType = assetGroup.GetAssetType("Circuit Location"))
          using (CircuitManager circuitManager = utilityNetwork.GetCircuitManager(telecomDomainNetwork))
          {
            // Create telecom elements Asset Type and GUID
            TelecomElement telcoStartElement = utilityNetwork.CreateElement(assetType, new Guid(startGlobalID)) as TelecomElement;
            TelecomElement telcoStopElement = utilityNetwork.CreateElement(assetType, new Guid(stopGlobalID)) as TelecomElement;

            // Describe the circuit locations on the start and stop elements.
            // This includes which ports on the element are being used for this circuit.            
            CircuitLocation startCircuitLocation = new CircuitLocation(telcoStartElement)
            {
              // Out of 16 ports (FirstUnit 1, LastUnit 16) only 5 are being used for this circuit, so FirstUnit is 1 and LastUnit is 5.
              FirstUnit = 1,
              LastUnit = 5
            };

            CircuitLocation stopCircuitLocation = new CircuitLocation(telcoStopElement)
            {
              // Out of 16 ports (FirstUnit 1, LastUnit 16) only 5 are being used for this circuit, so FirstUnit is 1 and LastUnit is 5.
              FirstUnit = 1,
              LastUnit = 5
            };

            // Create a circuit 
            using (Circuit circuit = new Circuit(circuitManager))
            {
              circuit.SetName(CircuitName);
              circuit.SetIsSectioned(false);
              circuit.SetCircuitType(CircuitType.Physical);
              circuit.SetStartLocation(startCircuitLocation);
              circuit.SetStopLocation(stopCircuitLocation);
              // .. set other properties as needed

              // circuitManager.Create(circuit); // For Corehost apps

              circuitManager.CreateInEditOperation(circuit);
            }
          }
        }
      });
    }

    /// <summary>
    /// Modifies the existing circuit
    /// Example modifies the circuit to have a subcircuit
    /// </summary>
    private void AlterCircuit()
    {
      QueuedTask.Run(() =>
      {
        using (Geodatabase geodatabase = _featureLayer.GetFeatureClass().GetDatastore() as Geodatabase)
        using (UtilityNetwork utilityNetwork = geodatabase.OpenDataset<UtilityNetwork>(_utilityNetworkName))
        using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
        {
          TelecomDomainNetwork telecomDomainNetwork = utilityNetworkDefinition.GetDomainNetwork("Telco") as TelecomDomainNetwork;
          using (CircuitManager circuitManager = utilityNetwork.GetCircuitManager(telecomDomainNetwork))
          {
            CircuitFilter circuitFilter = new CircuitFilter([CircuitName]);
            using (Circuit circuitToAlter = circuitManager.GetCircuits(circuitFilter).First())
            {
              var subcircuits = circuitToAlter.GetSubcircuits();

              using (Subcircuit subcircuit200Ghz = new Subcircuit(circuitManager))
              {
                subcircuit200Ghz.SetName("200GHzSubcircuit");
                // .. set other properties as needed

                // Add the subcircuit to the circuit
                circuitToAlter.SetSubcircuits(new List<Subcircuit>() { subcircuit200Ghz });

                // circuitManager.Alter(circuitToAlter); // For Corehost apps
                circuitManager.AlterInEditOperation(circuitToAlter);
              }
            }

            // Query after adding the subcircuit
            using (Circuit alteredCircuit = circuitManager.GetCircuits(circuitFilter).First())
            {
              var circuitName = alteredCircuit.GetName();
              var subcircuits = alteredCircuit.GetSubcircuits().FirstOrDefault().GetName();
            }
          }
        }
      });
    }

    /// <summary>
    /// Verifies the integrity of a circuit. 
    /// This includes checks such as whether the start and stop locations are valid
    /// State of the circuit (Dirty, Clean, Invalid) etc.
    /// </summary>
    private void VerifyCircuit()
    {
      QueuedTask.Run(() =>
      {
        using (Geodatabase geodatabase = _featureLayer.GetFeatureClass().GetDatastore() as Geodatabase)
        using (UtilityNetwork utilityNetwork = geodatabase.OpenDataset<UtilityNetwork>(_utilityNetworkName))
        using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
        {
          TelecomDomainNetwork telecomDomainNetwork = utilityNetworkDefinition.GetDomainNetwork("Telco") as TelecomDomainNetwork;
          using (CircuitManager circuitManager = utilityNetwork.GetCircuitManager(telecomDomainNetwork))
          {
            // circuitManager.Verify([CircuitName]); // For Corehost apps
            IReadOnlyList<VerifyCircuitResult> verifyCircuitResults = circuitManager.VerifyInEditOperation([CircuitName], false, null);

            // Get first result for demonstration purposes
            VerifyCircuitResult verifyCircuitResult = verifyCircuitResults.FirstOrDefault();
          }
        }
      });
    }

    /// <summary>
    /// This operation will change the circuit state to deleted. 
    /// The circuit will not be removed from the circuit table until the circuit is exported with acknowledgement. 
    /// </summary>
    private void DeleteCircuit()
    {
      QueuedTask.Run(() =>
      {
        using (Geodatabase geodatabase = _featureLayer.GetFeatureClass().GetDatastore() as Geodatabase)
        using (UtilityNetwork utilityNetwork = geodatabase.OpenDataset<UtilityNetwork>(_utilityNetworkName))
        using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
        {
          TelecomDomainNetwork telecomDomainNetwork = utilityNetworkDefinition.GetDomainNetwork("Telco") as TelecomDomainNetwork;
          using (CircuitManager circuitManager = utilityNetwork.GetCircuitManager(telecomDomainNetwork))
          {
            // circuitManager.Delete([CircuitName]); // For Corehost apps
            circuitManager.DeleteInEditOperation([CircuitName]);
          }
        }
      });

      // 
    }


    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class CircuitManagementPane_ShowButton : Button
  {
    protected override void OnClick()
    {
      CircuitManagementPaneViewModel.Show();
    }
  }
}
