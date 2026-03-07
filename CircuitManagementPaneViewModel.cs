using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CircuitManagementDemo
{
  internal class CircuitManagementPaneViewModel : DockPane
  {
    private const string _dockPaneID = "CircuitManagementDemo_CircuitManagementPane";

    protected CircuitManagementPaneViewModel() { }

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

    private void CreateCircuit()
    {
      var pp = CircuitName;
    }
    private void AlterCircuit()
    {
      var pp = CircuitName;

    }
    private void VerifyCircuit()
    {
      var pp = CircuitName;

    }
    private void DeleteCircuit()
    {
      var pp = CircuitName;

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
