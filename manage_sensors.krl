ruleset manage_sensors {
  meta {
    shares __testing, sensors, getTemps
    use module io.picolabs.wrangler alias Wrangler
  }
  global {
    __testing = { "queries":
      [ { "name": "__testing" }
      //, { "name": "entry", "args": [ "key" ] }
      ] , "events":
      [ //{ "domain": "d1", "type": "t1" }
      //, { "domain": "d2", "type": "t2", "attrs": [ "a1", "a2" ] }
      ]
    }
    
    threshold = 75
    smsNumber = "17072801567"
    location = "Provo, UT"
    
    sensors = function() {
      ent:sensors
    }
    
    getTemps = function() {
      ent:sensors.map(function(v,k){Wrangler:skyQuery(v["eci"],"temperature_store", "temperatures")})
    }
    
    
  }
  
  
  rule create_sensor {
    select when sensor new_sensor
    pre {
      name = event:attrs["sensor_name"]
      exists = ent:sensors >< name
    }
    if not exists
    then
      noop()
    fired {
      raise wrangler event "child_creation"
      attributes { "name": name,
                   "color": "#ffff00",
                   "rids": ["temperature_store", "wovyn_base", "sensor_profile"] }
    }
  }
  
  rule install_sensor {
    select when wrangler child_initialized
    pre {
      the_sensor = {"id": event:attrs["id"], "eci": event:attrs["eci"]}
      sensor_name = event:attrs["rs_attrs"]["name"]
    }
    if sensor_name.klog("Created new sensor: ")
    then 
      event:send({ "eci" : the_sensor["eci"], "eid" : "profile_updated", "domain": "sensor", "type": "profile_updated", "attrs" : { "smsNumber" : smsNumber, "sensorLocation" : location, "sensorName": sensor_name, "threshold" : threshold }});
    fired {
      ent:sensors := ent:sensors.defaultsTo({});
      ent:sensors{[sensor_name]} := the_sensor
    }
  }
  
  rule delete_sensor {
    select when sensor unneeded_sensor
    pre {
      name = event:attrs["sensor_name"]
      exists = ent:sensors >< name
    }
    if exists then 
      send_directive("Removing sensor", {"sensor_name" : name})
    fired {
      raise wrangler event "child_deletion"
        attributes {"name" : name};
      clear ent:sensors{[name]};
    }
  }
  
}
